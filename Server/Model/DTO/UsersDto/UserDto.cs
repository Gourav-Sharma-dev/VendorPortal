namespace Server.Model.DTO.UsersDto
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public bool IsActive { get; set; }
        public required string Role { get; set; }          // comma-joined (kept for compatibility)
        public List<string> Roles { get; set; } = [];      // individual roles list
        public DateTime CreatedAt { get; set; }
    }
}
