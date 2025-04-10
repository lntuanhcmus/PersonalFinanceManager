using ClosedXML.Excel;
using PersonalFinanceManager.Shared.Models;
using System.Globalization;

namespace PersonalFinanceManager.API.Services
{
    public class ExcelService
    {
        private readonly string ExcelFilePath = "Data/Transaction.xlsx";

        public void SaveTransactions(List<Transaction> transactions)
        {
            var workbook = File.Exists(ExcelFilePath) ? new XLWorkbook(ExcelFilePath) : new XLWorkbook();

            var worksheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name == "Transactions")
                ?? workbook.Worksheets.Add("Transactions");

            if (string.IsNullOrEmpty(worksheet.Cell(1, 1).Value.ToString()))
            {
                worksheet.Cell(1, 1).Value = "Thời gian giao dịch";
                worksheet.Cell(1, 2).Value = "Số lệnh giao dịch";
                worksheet.Cell(1, 3).Value = "Tài khoản nguồn";
                worksheet.Cell(1, 4).Value = "Tài khoản người hưởng";
                worksheet.Cell(1, 5).Value = "Tên người hưởng";
                worksheet.Cell(1, 6).Value = "Tên ngân hàng hưởng";
                worksheet.Cell(1, 7).Value = "Số tiền";
                worksheet.Cell(1, 8).Value = "Nội dung chuyển tiền";
                worksheet.Cell(1, 9).Value = "Loại giao dịch";
            }

            int row = worksheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
            var usedTransactionIds = worksheet.RowsUsed()
                .Skip(1)
                .Select(r => r.Cell(2).Value.ToString())
                .ToList();

            foreach (var transaction in transactions)
            {
                if (usedTransactionIds.Contains(transaction.TransactionId)) continue;

                worksheet.Cell(row, 1).Value = transaction.TransactionTime.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(row, 2).Value = transaction.TransactionId;
                worksheet.Cell(row, 3).Value = transaction.SourceAccount;
                worksheet.Cell(row, 4).Value = transaction.RecipientAccount;
                worksheet.Cell(row, 5).Value = transaction.RecipientName;
                worksheet.Cell(row, 6).Value = transaction.RecipientBank;
                worksheet.Cell(row, 7).Value = transaction.Amount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " VND"; ;
                worksheet.Cell(row, 8).Value = transaction.Description;
                worksheet.Cell(row, 9).Value = transaction.Category;
                row++;
                usedTransactionIds.Add(transaction.TransactionId);
            }

            workbook.SaveAs(ExcelFilePath);
        }

        public List<Transaction> GetTransactions()
        {
            if (!File.Exists(ExcelFilePath)) return new List<Transaction>();

            var workbook = new XLWorkbook(ExcelFilePath);
            var worksheet = workbook.Worksheet("Transactions");
            return worksheet.RowsUsed()
                .Skip(1)
                .Select(row => new Transaction
                {
                    TransactionTime = DateTime.ParseExact(row.Cell(1).Value.ToString(), "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                    TransactionId = row.Cell(2).Value.ToString(),
                    SourceAccount = row.Cell(3).Value.ToString(),
                    RecipientAccount = row.Cell(4).Value.ToString(),
                    RecipientName = row.Cell(5).Value.ToString(),
                    RecipientBank = row.Cell(6).Value.ToString(),
                    Amount = decimal.Parse(row.Cell(7).Value.ToString().Replace(" VND", "").Replace(".", ""), CultureInfo.InvariantCulture),
                    Description = row.Cell(8).Value.ToString(),
                    Category = row.Cell(9).Value.ToString()
                })
                .OrderByDescending(t => t.TransactionTime)
                .ToList();
        }

        public void SaveAllTransactions(List<Transaction> transactions)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Transactions");

            worksheet.Cell(1, 1).Value = "Thời gian giao dịch";
            worksheet.Cell(1, 2).Value = "Số lệnh giao dịch";
            worksheet.Cell(1, 3).Value = "Tài khoản nguồn";
            worksheet.Cell(1, 4).Value = "Tài khoản người hưởng";
            worksheet.Cell(1, 5).Value = "Tên người hưởng";
            worksheet.Cell(1, 6).Value = "Tên ngân hàng hưởng";
            worksheet.Cell(1, 7).Value = "Số tiền";
            worksheet.Cell(1, 8).Value = "Nội dung chuyển tiền";
            worksheet.Cell(1, 9).Value = "Loại giao dịch";

            int row = 2;
            foreach (var transaction in transactions)
            {
                worksheet.Cell(row, 1).Value = transaction.TransactionTime.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(row, 2).Value = transaction.TransactionId;
                worksheet.Cell(row, 3).Value = transaction.SourceAccount;
                worksheet.Cell(row, 4).Value = transaction.RecipientAccount;
                worksheet.Cell(row, 5).Value = transaction.RecipientName;
                worksheet.Cell(row, 6).Value = transaction.RecipientBank;
                worksheet.Cell(row, 7).Value = transaction.Amount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " VND";
                worksheet.Cell(row, 8).Value = transaction.Description;
                worksheet.Cell(row, 9).Value = transaction.Category;
                row++;
            }

            workbook.SaveAs(ExcelFilePath);
        }
    }
}
