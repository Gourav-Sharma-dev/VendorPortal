namespace Server.Model.DTO.UsersDto
{
    public class UpdateUserRequest
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public bool IsActive { get; set; }
    }
}
