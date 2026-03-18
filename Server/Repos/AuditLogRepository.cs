using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Model;
using Server.Repos.Core;

namespace Server.Repos
{
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetByEntityAsync(string entity, Guid entityId);
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId);
    }
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entity, Guid entityId)
        {
            return await _context.AuditLogs
                .Where(a => a.Entity == entity && a.EntityId == entityId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId)
        {
            return await _context.AuditLogs
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }
    }
}
