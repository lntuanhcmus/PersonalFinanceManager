namespace PersonalFinanceManager.API.Model
{
    public class AppSettings
    {
        public GmailServiceSettings GmailServiceSettings { get; set; } = new GmailServiceSettings();
    }
    public class GmailServiceSettings
    {
        public int MaxResult { get; set; } = 10;
    }

}
