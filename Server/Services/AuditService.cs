using Server.Model;
using Server.Repos;

namespace Server.Services
{
    public interface IAuditService
    {
        Task LogAsync(Guid userId, string action, string entity, Guid entityId, string details);
    }
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task LogAsync(Guid userId, string action, string entity, Guid entityId, string details)
        {
            var auditLog = new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                Timestamp = DateTime.Now,
                Details = details
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _auditLogRepository.SaveChangesAsync();
        }
    }
}
