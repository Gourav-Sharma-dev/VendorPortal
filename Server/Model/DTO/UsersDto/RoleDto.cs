namespace Server.Model.DTO.UsersDto
{
    public class RoleDto
    {
        public Guid RoleId { get; set; }
        public required string RoleName { get; set; }
        public required string RoleDescription { get; set; }
    }
}
