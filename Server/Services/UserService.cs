using Server.Model.DTO.UsersDto;
using Server.Repos;

namespace Server.Services
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<bool> UpdateUserAsync(Guid userId, UpdateUserRequest request);
        Task<bool> DeleteUserAsync(Guid userId);
    }
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditService _auditService;

        public UserService(IUserRepository userRepository, IAuditService auditService)
        {
            _userRepository = userRepository;
            _auditService = auditService;
        }

        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetUserWithRolesAsync(userId);
            if (user == null) return null;

            var roleNames = user.UserRoles?
                .Where(ur => ur.Role != null && !string.IsNullOrWhiteSpace(ur.Role.RoleName))
                .Select(ur => ur.Role.RoleName)
                .ToList() ?? [];

            return new UserDto
            {
                UserId    = user.UserId,
                Username  = user.Username,
                Email     = user.Email,
                FullName  = user.FullName,
                IsActive  = user.IsActive,
                CreatedAt = user.CreatedAt,
                Role      = string.Join(",", roleNames),
                Roles     = roleNames
            };
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersWithRolesAsync();
            return users.Select(u =>
            {
                var roleNames = u.UserRoles?
                    .Where(ur => ur.Role != null && !string.IsNullOrWhiteSpace(ur.Role.RoleName))
                    .Select(ur => ur.Role.RoleName)
                    .ToList() ?? [];

                return new UserDto
                {
                    UserId    = u.UserId,
                    Username  = u.Username,
                    Email     = u.Email,
                    FullName  = u.FullName,
                    IsActive  = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    Role      = string.Join(",", roleNames),
                    Roles     = roleNames
                };
            });
        }

        public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.Email = request.Email;
            user.FullName = request.FullName;
            user.IsActive = request.IsActive;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
            await _auditService.LogAsync(userId, "UpdateUser", "User", userId, $"User {user.Username} updated");
            return true;
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.IsDeleted = true;
            user.IsActive = false;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            await _auditService.LogAsync(userId, "DeleteUser", "User", userId, $"User {user.Username} deleted"
            );

            return true;
        }
    }
}
