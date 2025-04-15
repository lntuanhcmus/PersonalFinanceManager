namespace PersonalFinanceManager.Scheduler.Models
{
    public class TransactionData
    {
        public string Description { get; set; }
        public float Amount { get; set; }
        public string TransactionTime { get; set; }
        public string CategoryId { get; set; }
        public string TransactionTypeId { get; set; }
        public string DayOfWeek { get; set; }
        public float HourOfDay { get; set; }
    }
}