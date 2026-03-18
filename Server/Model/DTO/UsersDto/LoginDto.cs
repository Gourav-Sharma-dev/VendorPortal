using System.ComponentModel.DataAnnotations;

namespace Server.Model.DTO.UsersDto
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public required string Username { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public required string Password { get; set; }
    }
}
