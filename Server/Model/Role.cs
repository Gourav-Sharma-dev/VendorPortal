using System.ComponentModel.DataAnnotations;

namespace Server.Model
{
    public class Role
    {
        [Key]
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? RoleDescription { get; set; } = string.Empty;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
