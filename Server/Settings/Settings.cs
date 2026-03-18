namespace Server.Settings
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryInMinutes { get; set; }
    }
    public class EncryptionSettings
    {
        public string Key { get; set; } = string.Empty;
        public string IV { get; set; } = string.Empty;
    }
}
