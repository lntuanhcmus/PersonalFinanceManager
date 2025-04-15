using Google.Apis.Auth.OAuth2;
using GMService = Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2.Flows;
using System.Text.Json;
using Google.Apis.Auth.OAuth2.Responses;
using HtmlAgilityPack;
using System.Globalization;
using PersonalFinanceManager.Shared.Enum;
using PersonalFinanceManager.Shared.Data.Entity;
using PersonalFinanceManager.Shared.Services;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Helpers;

namespace PersonalFinanceManager.Infrastructure.Services
{
    public class GmailService: IGmailService
    {
        private static readonly string[] Scopes = { GMService.GmailService.Scope.GmailReadonly };
        private static readonly string ApplicationName = "Personal Finance Manager";
        private readonly ITokenService _tokenService;
        private readonly string _provider = "GmailToken";


        public GmailService(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task<List<Transaction>> ExtractTransactionsAsync(string credentialsPath, int maxResult = 10, UserCredential? credentialData = null)
        {
            UserCredential credential;
            if (credentialData == null)
            {
                var existToken = await _tokenService.GetTokenAsync(_provider);
                TokenResponse? token;
                if (existToken == null)
                {
                    using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                    {
                        var clientSecrets = GoogleClientSecrets.FromStream(stream).Secrets;
                        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                        {
                            ClientSecrets = clientSecrets,
                            Scopes = Scopes
                        });
                        credential = await AuthorizeManually(flow);
                    }

                }
                else
                {
                    token = existToken.ToTokenResponse();
                    credential = await GetCredentialAsync(credentialsPath, token);
                }
            }
            else
            {
                credential = credentialData;
            }

            var service = new GMService.GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var messages = await GetMessagesAsync(service, maxResult);

            var transactions = await ExtractTransactionsFromMessagesAsync(service, messages);

            return transactions;
        }

        private async Task<UserCredential> GetCredentialAsync(string credentialsPath, TokenResponse? token)
        {
            if (token == null)
            {
                throw new Exception("Token is not provided");
            }

            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                var clientSecrets = GoogleClientSecrets.FromStream(stream).Secrets;
                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets,
                    Scopes = Scopes
                });

                UserCredential credential = new UserCredential(flow, "user", token);

                if (credential.Token.IsExpired(flow.Clock))
                {
                    await credential.RefreshTokenAsync(CancellationToken.None);
                    var newToken = credential.Token.ToExternalToken(_provider);
                    await _tokenService.SaveTokenAsync(newToken);
                }

                return credential;
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

        private async Task<List<Transaction>> ExtractTransactionsFromMessagesAsync(GMService.GmailService service, List<Google.Apis.Gmail.v1.Data.Message> messages)
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
                    Status = (int)TransactionStatusEnum.Success
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

        // Xác thực thủ công và lưu token
        private async Task<UserCredential> AuthorizeManually(GoogleAuthorizationCodeFlow flow)
        {
            var authUrl = flow.CreateAuthorizationCodeRequest("http://localhost:8000/oauth/callback");
            var baseUrl = authUrl.Build().ToString();

            // Kiểm tra kỹ: không nên nối thêm nếu baseUrl đã chứa access_type
            // Nếu không chắc, bạn có thể dùng URI builder để xử lý an toàn hơn
            string finalUrl = baseUrl.Contains("access_type=")
                ? baseUrl
                : baseUrl + "&access_type=offline";

            finalUrl = finalUrl.Contains("&prompt=") ? finalUrl : finalUrl + "&prompt=consent";

            Console.WriteLine("Mở URL sau để cấp quyền:");
            Console.WriteLine(finalUrl);
            // Nhập mã xác thực từ trình duyệt
            Console.Write("Nhập mã xác thực: ");
            string code = Console.ReadLine();

            // Đổi mã xác thực lấy token
            var token = await flow.ExchangeCodeForTokenAsync(
                "user",
                code,
                "http://localhost:8000/oauth/callback", // Phải giống tuyệt đối
                CancellationToken.None
            );

            // Lưu token

            var newToken = token.ToExternalToken(_provider);
            await _tokenService.SaveTokenAsync(newToken);

            // Trả về thông tin xác thực
            return new UserCredential(flow, "user", token);
        }






        public async Task<UserCredential> ExchangeCodeForTokenAsync(string code)
        {
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                var clientSecrets = GoogleClientSecrets.FromStream(stream).Secrets;
                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets,
                    Scopes = Scopes
                    // Không cần DataStore ở đây nữa
                });

                var token = await flow.ExchangeCodeForTokenAsync("user", code, "http://localhost:8000/oauth/callback", CancellationToken.None);
                var credential = new UserCredential(flow, "user", token);

                var newToken = token.ToExternalToken(_provider);
                await _tokenService.SaveTokenAsync(newToken);

                return credential;
            }
        }




        //// Hàm yêu cầu xác thực lại
        //private async Task<UserCredential> ReauthorizeUser(GoogleAuthorizationCodeFlow flow)
        //{
        //    var authUrl = flow.CreateAuthorizationCodeRequest("http://localhost:8000/oauth/callback").Build();
        //    Console.WriteLine("Mở URL sau để cấp quyền:");
        //    Console.WriteLine(authUrl.ToString());
        //    Console.WriteLine("Nhập code từ trình duyệt:");
        //    Console.Write("Code: ");
        //    string code = Console.ReadLine();

        //    var token = await flow.ExchangeCodeForTokenAsync("user", code, "http://localhost:8000/oauth/callback", CancellationToken.None);
        //    File.WriteAllText("token.json", JsonSerializer.Serialize(token));
        //    return new UserCredential(flow, "user", token);
        //}
    }
}