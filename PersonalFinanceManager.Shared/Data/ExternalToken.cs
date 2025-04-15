namespace PersonalFinanceManager.Shared.Data
{
    public class ExternalToken
    {
        public int Id { get; set; }

        public string Provider { get; set; } = default!;

        public string? UserEmail { get; set; } = default!;

        public string AccessToken { get; set; } = default!;

        public string? TokenType { get; set; }

        public string? RefreshToken { get; set; } = default!;

        public string? Scope { get; set; }

        public string? IdToken { get; set; }

        public DateTime Issued { get; set; }

        public DateTime IssuedUtc { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public long? ExpiresInSeconds { get; set; }

        public bool IsStale { get; set; }
    }
}
