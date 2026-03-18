using System.Data;
using Server.Model;
using Server.Model.DTO.AuditDto;
using Server.Model.DTO.UsersDto;
using Server.Repos;
using Server.Repos.Core;

namespace Server.Services
{
    public interface IAdminService
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(AuditLogFilter filter);
        Task<bool> GrantAccessAsync(string grantedByUserId, Guid grantedToUserId, Guid roleId);
        Task<bool> RevokeAccessAsync(string revokedByUserId, Guid revokedFromUserId, Guid roleId);
    }

    public class AdminService : IAdminService
    {
        private readonly IGenericRepository<Role> _roleRepository;
        private readonly IGenericRepository<UserRole> _userRoleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IAuditService _auditService;

        public AdminService(
            IGenericRepository<Role> roleRepository,
            IGenericRepository<UserRole> userRoleRepository,
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository,
            IAuditService auditService)
        {
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
            _auditService = auditService;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(r => new RoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                RoleDescription = r.RoleDescription ?? string.Empty
            });
        }

        public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(AuditLogFilter filter)
        {
            var logs = await _auditLogRepository.GetAllAsync();

            // Default to last 7 days if no date range specified
            if (!filter.FromDate.HasValue && !filter.ToDate.HasValue)
            {
                filter.FromDate = DateTime.UtcNow.AddDays(-7).Date;
                filter.ToDate = DateTime.UtcNow.Date.AddDays(1); // include today
            }

            if (!string.IsNullOrEmpty(filter.Entity))
                logs = logs.Where(l => l.Entity == filter.Entity);

            if (filter.EntityId.HasValue)
                logs = logs.Where(l => l.EntityId == filter.EntityId.Value);

            if (filter.UserId.HasValue)
                logs = logs.Where(l => l.UserId == filter.UserId.Value);

            if (!string.IsNullOrEmpty(filter.Action))
                logs = logs.Where(l => l.Action.Contains(filter.Action, StringComparison.OrdinalIgnoreCase));

            if (filter.FromDate.HasValue)
                logs = logs.Where(l => l.Timestamp >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                logs = logs.Where(l => l.Timestamp < filter.ToDate.Value);

            // Pre-load users to resolve usernames
            var allUsers = (await _userRepository.GetAllAsync()).ToList();

            // Apply username filter after resolving names
            List<AuditLogDto> logDtos = logs.OrderByDescending(l => l.Timestamp)
                                            .Select(l =>
                                            {
                                                var user = l.UserId.HasValue ? allUsers.FirstOrDefault(u => u.UserId == l.UserId.Value) : null;
                                                return new AuditLogDto
                                                {
                                                    Action = l.Action,
                                                    Entity = l.Entity,
                                                    EntityId = l.EntityId,
                                                    Timestamp = l.Timestamp,
                                                    Details = l.Details,
                                                    UserId = l.UserId,
                                                    Username = user?.Username ?? l.UserId?.ToString() ?? "System"
                                                };
                                            }).ToList();

            // Filter by username after resolution
            if (!string.IsNullOrEmpty(filter.Username))
                logDtos = logDtos.Where(l => l.Username.Contains(filter.Username, StringComparison.OrdinalIgnoreCase)).ToList();

            return logDtos;
        }

        public async Task<bool> GrantAccessAsync(string grantedByUserId, Guid grantedToUserId, Guid roleId)
        {
            var user = await _userRepository.GetByIdAsync(grantedToUserId);
            if (user == null) return false;

            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null) return false;

            // Check if user already has this role
            var existingUserRole = (await _userRoleRepository.GetAllAsync())
                .FirstOrDefault(ur => ur.UserId == grantedToUserId && ur.RoleId == roleId);

            if (existingUserRole != null)
                return false;

            var userRole = new UserRole
            {
                UserId = grantedToUserId,
                RoleId = roleId
            };

            await _userRoleRepository.AddAsync(userRole);
            await _userRoleRepository.SaveChangesAsync();

            await _auditService.LogAsync(
                Guid.Parse(grantedByUserId),
                "GrantAccess",
                "UserRole",
                grantedToUserId,
                $"Role '{role.RoleName}' granted to user '{user.Username}'"
            );

            return true;
        }

        public async Task<bool> RevokeAccessAsync(string revokedByUserId, Guid revokedFromUserId, Guid roleId)
        {
            var user = await _userRepository.GetByIdAsync(revokedFromUserId);
            if (user == null) return false;

            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null) return false;

            var userRole = (await _userRoleRepository.GetAllAsync())
                .FirstOrDefault(ur => ur.UserId == revokedFromUserId && ur.RoleId == roleId);

            if (userRole == null)
                return false;

            await _userRoleRepository.DeleteEntityAsync(userRole);
            await _userRoleRepository.SaveChangesAsync();

            await _auditService.LogAsync(
                Guid.Parse(revokedByUserId),
                "RevokeAccess",
                "UserRole",
                revokedFromUserId,
                $"Role '{role.RoleName}' revoked from user '{user.Username}'"
            );

            return true;
        }
    }
}
