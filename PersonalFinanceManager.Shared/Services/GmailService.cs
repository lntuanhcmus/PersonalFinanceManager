using Google.Apis.Auth.OAuth2;
using GMService = Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2.Flows;
using System.Text.Json;
using Google.Apis.Auth.OAuth2.Responses;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PersonalFinanceManager.Shared.Models;
using ClosedXML.Excel;
using System.Globalization;
using Google.Apis.Util.Store;
using Google.Apis.Auth.OAuth2.Requests;
using Newtonsoft.Json.Linq;

namespace PersonalFinanceManager.Shared.Services
{
    public class GmailService
    {
        private static readonly string[] Scopes = { GMService.GmailService.Scope.GmailReadonly };
        private static readonly string ApplicationName = "Personal Finance Manager";
        private static readonly string ExcelFilePath = "Data/transactions.xlsx";

        public async Task<List<Transaction>> ExtractTransactionsAsync(string credentialsPath, string tokenPath)
        {
            UserCredential credential;

            //// Khởi tạo credential với FileDataStore
            //using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            //{
            //    var receiver = new LocalServerCodeReceiver(8000); // Port cố định
            //    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            //        GoogleClientSecrets.FromStream(stream).Secrets,
            //        Scopes,
            //        "user",
            //        CancellationToken.None,
            //        new FileDataStore(tokenPath, true),
            //        receiver);
            //}

            TokenResponse token = null;
            if (File.Exists(tokenPath))
            {
                string tokenJson = await File.ReadAllTextAsync(tokenPath);
                token = JsonSerializer.Deserialize<TokenResponse>(tokenJson);
            }

            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                var clientSecrets = GoogleClientSecrets.FromStream(stream).Secrets;
                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets,
                    Scopes = Scopes
                });

                // Nếu không có token hoặc token hết hạn, yêu cầu xác thực mới
                if (token == null || IsTokenExpired(token))
                {
                    credential = await AuthorizeManually(flow, tokenPath);
                }
                else
                {
                    credential = new UserCredential(flow, "user", token);
                    // Kiểm tra và refresh token nếu cần
                    if (credential.Token.IsExpired(flow.Clock))
                    {
                        await credential.RefreshTokenAsync(CancellationToken.None);
                        await SaveTokenManually(credential.Token, tokenPath); // Lưu token sau khi refresh
                    }
                }
            }

            Console.WriteLine($"Access Token: {credential.Token.AccessToken}");
            Console.WriteLine($"Expires In: {credential.Token.ExpiresInSeconds}");
            Console.WriteLine($"Refresh Token: {credential.Token.RefreshToken}");

            var service = new GMService.GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var request = service.Users.Messages.List("me");
            request.Q = "from:vietcombank Biên lai chuyển tiền qua tài khoản";
            request.MaxResults = 10;

            var messages = (await request.ExecuteAsync()).Messages ?? new List<Google.Apis.Gmail.v1.Data.Message>();
            var transactions = new List<Transaction>();

            foreach (var msg in messages)
            {
                var message = await service.Users.Messages.Get("me", msg.Id).ExecuteAsync();
                string content = message.Payload.Parts?.FirstOrDefault()?.Body.Data ?? message.Payload.Body.Data;
                content = content != null ? Base64UrlDecode(content) : "N/A";
    

                var doc = new HtmlDocument();
                doc.LoadHtml(content);
                var keysToExtract = new List<string>
                {
                    "Ngày, giờ giao dịch",
                    "Số lệnh giao dịch",
                    "Tài khoản nguồn",
                    "Tài khoản người hưởng",
                    "Tên người hưởng",
                    "Tên ngân hàng hưởng",
                    "Số tiền",
                    "Nội dung chuyển tiền"
                };
                Dictionary<string,string> result = ExtractFieldsByKeys(doc, keysToExtract);
                string transactionId = result["Số lệnh giao dịch"];

                transactions.Add(new Transaction
                {
                    TransactionTime = DateTime.ParseExact(result["Ngày, giờ giao dịch"], "HH:mm dddd dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN")),
                    TransactionId = transactionId,
                    SourceAccount = result["Tài khoản nguồn"],
                    RecipientAccount = result["Tài khoản người hưởng"],
                    RecipientName = result["Tên người hưởng"],
                    RecipientBank = result["Tên ngân hàng hưởng"],
                    Amount = decimal.Parse(result["Số tiền"].Replace(" VND", "").Replace(",", ""), CultureInfo.InvariantCulture),
                    Description = result["Nội dung chuyển tiền"],
                    Category = "Chi"
                });
            }

            return transactions;
            
            //workbook.SaveAs(ExcelFilePath);
            //Console.WriteLine("Giao dịch đã được lưu vào " + ExcelFilePath);
        }

        // Xác thực thủ công và lưu token
        private async Task<UserCredential> AuthorizeManually(GoogleAuthorizationCodeFlow flow, string TokenFilePath)
        {
            var authUrl = flow.CreateAuthorizationCodeRequest("http://localhost:8000/oauth/callback").Build();
            Console.WriteLine("Mở URL sau để cấp quyền:");
            Console.WriteLine(authUrl.ToString());
            Console.WriteLine("Nhập code từ trình duyệt:");
            Console.Write("Code: ");
            string code = Console.ReadLine();

            var token = await flow.ExchangeCodeForTokenAsync("user", code, "http://localhost:8000/oauth/callback", CancellationToken.None);
            await SaveTokenManually(token, TokenFilePath);

            return new UserCredential(flow, "user", token);
        }

        // Lưu token thủ công vào file
        private async Task SaveTokenManually(TokenResponse token, string TokenFilePath)
        {
            string tokenJson = JsonSerializer.Serialize(token, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(TokenFilePath, tokenJson);
            Console.WriteLine($"Token đã được lưu vào {TokenFilePath}");
        }

        // Kiểm tra token có hết hạn không
        private bool IsTokenExpired(TokenResponse token)
        {
            return token.ExpiresInSeconds.HasValue && DateTime.UtcNow >= token.IssuedUtc.AddSeconds(token.ExpiresInSeconds.Value);
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

                // Lưu token trực tiếp vào file token.json
                string tokenJson = JsonSerializer.Serialize(token);
                File.WriteAllText("token.json", tokenJson);

                return credential;
            }
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
    
    
        // Hàm yêu cầu xác thực lại
        private async Task<UserCredential> ReauthorizeUser(GoogleAuthorizationCodeFlow flow)
        {
            var authUrl = flow.CreateAuthorizationCodeRequest("http://localhost:8000/oauth/callback").Build();
            Console.WriteLine("Mở URL sau để cấp quyền:");
            Console.WriteLine(authUrl.ToString());
            Console.WriteLine("Nhập code từ trình duyệt:");
            Console.Write("Code: ");
            string code = Console.ReadLine();

            var token = await flow.ExchangeCodeForTokenAsync("user", code, "http://localhost:8000/oauth/callback", CancellationToken.None);
            File.WriteAllText("token.json", JsonSerializer.Serialize(token));
            return new UserCredential(flow, "user", token);
        }
    }
}