namespace Server.Model.DTO.AuditDto
{
    public class AuditLogDto
    {
        public required string Action { get; set; }
        public required string Entity { get; set; }
        public Guid EntityId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }
        public required string Username { get; set; }
        public Guid? UserId { get; set; }
    }
}
