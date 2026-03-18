using System.ComponentModel.DataAnnotations;

namespace Server.Model.DTO.UsersDto
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public required string Email { get; set; }
        
        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character")]
        public required string NewPassword { get; set; }
        
        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, MinimumLength = 4, ErrorMessage = "OTP must be between 4 and 6 characters")]
        public required string OTP { get; set; }
    }
}
