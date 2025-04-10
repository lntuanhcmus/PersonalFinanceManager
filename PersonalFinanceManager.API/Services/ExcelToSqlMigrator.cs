namespace PersonalFinanceManager.API.Services
{
    public class ExcelToSqlMigrator
    {
        private readonly ExcelService _excelService;
        private readonly TransactionService _transactionService;

        public ExcelToSqlMigrator(ExcelService excelService, TransactionService transactionService)
        {
            _excelService = excelService;
            _transactionService = transactionService;
        }

        public async Task Migrate()
        {
            var transactions = _excelService.GetTransactions();
            await _transactionService.SaveTransactions(transactions);
        }
    }
}
