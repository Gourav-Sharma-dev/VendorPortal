using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Server.Model.DTO.AuditDto;
using Server.Services;

namespace Server.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// Get all roles in the system
        /// </summary>
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _adminService.GetAllRolesAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Get audit logs with optional filtering (defaults to last 7 days)
        /// </summary>
        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilter filter)
        {
            var logs = await _adminService.GetAuditLogsAsync(filter);
            return Ok(logs);
        }

        /// <summary>
        /// Grant role access to a user
        /// </summary>
        [HttpPost("grant-access")]
        public async Task<IActionResult> GrantAccess([FromQuery] Guid targetUserId, [FromQuery] Guid roleId)
        {
            var grantedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(grantedByUserId))
                return Unauthorized();

            var result = await _adminService.GrantAccessAsync(grantedByUserId, targetUserId, roleId);
            if (!result) return Conflict(new { message = "User or role not found, or role is already assigned." });

            return Ok(new { message = "Role assigned successfully." });
        }

        [HttpDelete("revoke-access")]
        public async Task<IActionResult> RevokeAccess([FromQuery] Guid targetUserId, [FromQuery] Guid roleId)
        {
            var revokedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(revokedByUserId))
                return Unauthorized();

            var result = await _adminService.RevokeAccessAsync(revokedByUserId, targetUserId, roleId);
            if (!result) return NotFound(new { message = "User role assignment not found." });

            return Ok(new { message = "Role revoked successfully." });
        }
    }
}
