namespace Server.Model.DTO
{
    public class AuthResponse
    {
        public required string Token { get; set; }
        public required string Username { get; set; }
        public required string Role { get; set; }
        public string? Message { get; set; }
    }
}
