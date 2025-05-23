using Google.Apis.Auth.OAuth2;
using GMService = Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2.Flows;
using System.Text.Json;
using Google.Apis.Auth.OAuth2.Responses;
using HtmlAgilityPack;
using System.Globalization;
using PersonalFinanceManager.Shared.Enum;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Infrastructure.Repositories;

namespace PersonalFinanceManager.Infrastructure.Services
{
    public class GmailService: IGmailService
    {
        private static readonly string[] Scopes = { GMService.GmailService.Scope.GmailReadonly, "https://www.googleapis.com/auth/userinfo.email" };
        private static readonly string ApplicationName = "Personal Finance Manager";
        private readonly IExternalTokenService _tokenService;
        private readonly string _provider = "GmailToken";
        private readonly IUserRepository _userRepository;


        public GmailService(IExternalTokenService tokenService, IUserRepository userRepository)
        {
            _tokenService = tokenService;
            _userRepository = userRepository;
        }

        //Main Action

        // Khởi tạo luồng OAuth và trả về URL để người dùng cấp quyền
        public async Task<string> InitiateOAuthFlowAsync(string userId, string credentialsPath, string redirectUri)
        {
            using var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);
            var clientSecrets = GoogleClientSecrets.FromStream(stream).Secrets;
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = clientSecrets,
                Scopes = Scopes
            });

            var authUrl = flow.CreateAuthorizationCodeRequest(redirectUri);
            return authUrl.Build().ToString() + "&prompt=consent&state=" + userId.ToString();
        }

        public async Task<UserCredential> ExchangeCodeForTokenAsync(string userId, string credentialsPath, string code, string redirectUri)
        {
            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                var clientSecrets = GoogleClientSecrets.FromStream(stream).Secrets;
                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets,
                    Scopes = Scopes
                    // Không cần DataStore ở đây nữa
                });

                var token = await flow.ExchangeCodeForTokenAsync(userId, code, redirectUri, CancellationToken.None);
                var credential = new UserCredential(flow, userId, token);

                var service = new GMService.GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });

                var userProfile = await service.Users.GetProfile("me").ExecuteAsync();
                var userEmail = userProfile.EmailAddress;

                var user = await _userRepository.FindByIdAsync(Int16.Parse(userId));

                if(user != null && user.Email == userEmail)
                {
                    var externalToken = new ExternalToken
                    {
                        Provider = _provider,
                        UserEmail = userEmail,
                        AccessToken = token.AccessToken,
                        RefreshToken = token.RefreshToken,
                        TokenType = token.TokenType,
                        Scope = token.Scope,
                        IdToken = token.IdToken,
                        Issued = token.Issued,
                        IssuedUtc = token.IssuedUtc,
                        ExpiresAtUtc = token.IssuedUtc.AddSeconds(token.ExpiresInSeconds ?? 3600),
                        ExpiresInSeconds = token.ExpiresInSeconds,
                        IsStale = false,
                        UserId = user.Id,
                    };
                    await _tokenService.SaveTokenAsync(externalToken);

                    user.IsConnectedGmail = true;
                    await _userRepository.SaveChangeAsync(user);
                }
                else
                {
                    throw new Exception("Người dùng không khớp với email từ Gmail API");
                }

                return credential;
            }
        }

        public async Task<UserCredential> GetCredentialFromToken(string userId, string credentialsPath)
        {
            var user = await _userRepository.FindByIdAsync(int.Parse(userId));
            if (user == null)
                throw new Exception("Không tìm thấy người dùng");

            var token = await _tokenService.GetTokenAsync(_provider, user.Email);
            if (token == null)
                throw new Exception("Không tìm thấy token Gmail cho người dùng");

            var credential = await GetCredentialAsync(credentialsPath, token.ToTokenResponse(), userId);

            return credential;
        }

        public async Task<List<Transaction>> ExtractTransactionsAsync(string userId, UserCredential credential, int maxResult = 10)
        {
            var service = new GMService.GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var messages = await GetMessagesAsync(service, maxResult);

            var transactions = await ExtractTransactionsFromMessagesAsync(service, messages, int.Parse(userId));

            return transactions;
        }


        //Common Function


        private async Task<UserCredential> GetCredentialAsync(string credentialsPath, TokenResponse token, string userId)
        {
            try
            {
                using var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);
                var clientSecrets = GoogleClientSecrets.FromStream(stream).Secrets;
                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets,
                    Scopes = Scopes
                });

                var credential = new UserCredential(flow, userId, token);
                if (credential.Token.IsExpired(flow.Clock))
                {
                    if (string.IsNullOrEmpty(token.RefreshToken))
                        throw new Exception("Refresh token không tồn tại, yêu cầu người dùng xác thực lại");

                    await credential.RefreshTokenAsync(CancellationToken.None);
                    var newToken = credential.Token.ToExternalToken(_provider);
                    newToken.UserId = int.Parse(userId); // Chuyển từ string sang int
                    await _tokenService.SaveTokenAsync(newToken);
                }
                return credential;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private async Task<List<GMService.Data.Message>> GetMessagesAsync(GMService.GmailService service, int maxResult)
        {
            var request = service.Users.Messages.List("me");
            request.Q = "from:vietcombank Biên lai chuyển tiền qua tài khoản";
            request.MaxResults = maxResult;

            var response = await request.ExecuteAsync();
            return (List<GMService.Data.Message>)(response.Messages ?? new List<GMService.Data.Message>());
        }

        private async Task<List<Transaction>> ExtractTransactionsFromMessagesAsync(GMService.GmailService service, List<Google.Apis.Gmail.v1.Data.Message> messages, int userId)
        {
            var transactions = new List<Transaction>();

            foreach (var msg in messages)
            {
                var message = await service.Users.Messages.Get("me", msg.Id).ExecuteAsync();
                string content = Base64UrlDecode(message.Payload.Parts?.FirstOrDefault()?.Body.Data ?? message.Payload.Body.Data);

                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                var result = ExtractFieldsByKeys(doc, new List<string>
            {
                "Ngày, giờ giao dịch", "Số lệnh giao dịch", "Tài khoản nguồn",
                "Tài khoản người hưởng", "Tên người hưởng", "Tên ngân hàng hưởng",
                "Số tiền", "Nội dung chuyển tiền"
            });

                transactions.Add(new Transaction
                {
                    TransactionTime = DateTime.ParseExact(result["Ngày, giờ giao dịch"], "HH:mm dddd dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN")),
                    TransactionId = result["Số lệnh giao dịch"],
                    SourceAccount = result["Tài khoản nguồn"],
                    RecipientAccount = result["Tài khoản người hưởng"],
                    RecipientName = result["Tên người hưởng"],
                    RecipientBank = result["Tên ngân hàng hưởng"],
                    Amount = decimal.Parse(result["Số tiền"].Replace(" VND", "").Replace(",", ""), CultureInfo.InvariantCulture),
                    Description = result["Nội dung chuyển tiền"],
                    TransactionTypeId = (int)TransactionTypeEnum.Expense,
                    Status = (int)TransactionStatusEnum.Success,
                    UserId = userId,
                });
            }

            return transactions;
        }

        private async Task SaveTokenManually(TokenResponse token, string TokenFilePath)
        {
            string tokenJson = JsonSerializer.Serialize(token, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(TokenFilePath, tokenJson);
            Console.WriteLine($"Token đã được lưu vào {TokenFilePath}");
        }

        private string Base64UrlDecode(string input)
        {
            string base64 = input.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            byte[] bytes = Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        private Dictionary<string, string> ExtractFieldsByKeys(HtmlDocument doc, List<string> keys)
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var rows = doc.DocumentNode.SelectNodes("//tr");

            if (rows == null) return data;

            foreach (var row in rows)
            {
                var cells = row.SelectNodes("td");
                if (cells == null || cells.Count < 2) continue;

                string label = cells[0].SelectSingleNode(".//b")?.InnerText.Trim() ?? "";
                string value = cells[1].InnerText.Trim();

                if (keys.Contains(label))
                {
                    data[label] = value;
                }
            }

            return data;
        }

        // Kiểm tra token có hết hạn không
        private bool IsTokenExpired(TokenResponse token)
        {
            return token.ExpiresInSeconds.HasValue && DateTime.UtcNow >= token.IssuedUtc.AddSeconds(token.ExpiresInSeconds.Value);
        }
    }
}