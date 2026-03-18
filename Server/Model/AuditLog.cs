using System.ComponentModel.DataAnnotations;

namespace Server.Model
{
    public class AuditLog
    {
        [Key]
        public Guid AuditLogId { get; set; }
        public Guid? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; } = string.Empty;

        //public User User { get; set; } = new();
    }
}
