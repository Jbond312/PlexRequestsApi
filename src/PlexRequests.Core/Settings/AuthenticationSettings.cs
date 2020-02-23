namespace PlexRequests.Core.Settings
{
    public class AuthenticationSettings
    {
        public string SecretKey { get; set; }
        public int TokenExpirationMinutes { get; set; }
        public int RefreshTokenExpirationDays { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
    }
}
