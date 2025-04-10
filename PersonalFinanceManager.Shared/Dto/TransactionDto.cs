namespace PersonalFinanceManager.Shared.Dto
{
    public class TransactionDto
    {
        public string TransactionId { get; set; }
        public string TransactionTime { get; set; } // Nhận dưới dạng chuỗi
        public string SourceAccount { get; set; }
        public string RecipientAccount { get; set; }
        public string RecipientName { get; set; }
        public string RecipientBank { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }
}
