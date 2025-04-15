namespace PersonalFinanceManager.Shared.Data
{
    public class LabelingRule
    {
        public int Id { get; set; }

        public string Keyword { get; set; }

        public int TransactionTypeId { get; set; }

        public int CategoryId { get; set; }
    }
}
