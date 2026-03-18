using Server.Model;
using Server.Model.DTO;
using Server.Model.DTO.UsersDto;
using Server.Repos;
using Server.Repos.Core;
using Server.Services.Common;

namespace Server.Services
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginDto request);
        Task<bool> RegisterAsync(RegisterDto request);
        Task LogoutAsync(string userId);
        Task<bool> ResetPasswordAsync(ResetPasswordDto request);
    }

    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<Role> _roleRepository;
        private readonly IGenericRepository<UserRole> _userRoleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditService _auditService;
        private readonly IJwtService _jwtService;
        private readonly ICryptoService _cryptoService;

        private const string VENDOR = "Vendor";

        public AuthService(IGenericRepository<Role> roleRepository, IGenericRepository<UserRole> userRoleRepository, IUserRepository userRepository, IAuditService auditService, IJwtService jwtService, ICryptoService cryptoService)
        {
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
            _auditService = auditService;
            _jwtService = jwtService;
            _cryptoService = cryptoService;
        }

        public async Task<AuthResponse?> LoginAsync(LoginDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return null;
            }

            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                return null;
            }

            var userWithRoles = await _userRepository.GetUserWithRolesAsync(user.UserId);
            if (userWithRoles == null || !_cryptoService.VerifyPassword(request.Password, userWithRoles.PasswordHash))
            {
                return null;
            }

            if (!userWithRoles.IsActive)
            {
                await _auditService.LogAsync(userWithRoles.UserId, "LoginFailed", "User", userWithRoles.UserId, $"Inactive user login attempt: {request.Username}");
                return null;
            }

            var roles = userWithRoles.UserRoles?.Select(ur => ur.Role?.RoleName).Where(r => r != null).ToList() ?? [];
            var token = _jwtService.GenerateToken(userWithRoles.UserId, userWithRoles.Username, roles);

            await _auditService.LogAsync(userWithRoles.UserId, "LoginSuccess", "User", userWithRoles.UserId, $"User {userWithRoles.Username} logged in");

            return new AuthResponse
            {
                Token = token,
                Username = userWithRoles.Username,
                Role = roles.Count > 0 ? string.Join(",", roles) : "User",
                Message = "Login successful"
            };
        }

        public async Task<bool> RegisterAsync(RegisterDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.FullName))
            {
                return false;
            }

            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return false;
            }

            var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                return false;
            }
            var vendorRole = (await _roleRepository.GetAllAsync()).FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.RoleName) && r.RoleName.Equals(VENDOR, StringComparison.OrdinalIgnoreCase));

            if (vendorRole == null)
            {
                // Log error: Vendor role not found in database
                return false;
            }
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = _cryptoService.HashPassword(request.Password),
                Email = request.Email,
                FullName = request.FullName,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var userRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = vendorRole.RoleId
            };

            await _userRepository.AddAsync(user);
            await _userRoleRepository.AddAsync(userRole);
            await _userRepository.SaveChangesAsync();

            await _auditService.LogAsync(user.UserId, "RegisterUser", "User", user.UserId, $"User {user.Username} registered");

            return true;
        }

        public async Task LogoutAsync(string userId)
        {
            await _auditService.LogAsync(Guid.Parse(userId), "Logout", "User", Guid.Parse(userId), $"User with ID {userId} logged out");
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.NewPassword) ||
                string.IsNullOrWhiteSpace(request.OTP))
            {
                return false;
            }

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return false;
            }

            user.PasswordHash = _cryptoService.HashPassword(request.NewPassword);

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
            await _auditService.LogAsync(user.UserId, "ResetPassword", "User", user.UserId, $"Password reset for user: {user.Username}");

            return true;
        }
    }
}
