namespace PersonalFinanceManager.WebHost.Models
{
    public class AppSettings
    {
        public ApiSettings ApiSettings { get; set; } = new ApiSettings();
    }

    public class ApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;

        public int TimeoutSeconds { get; set; }
    }

}
