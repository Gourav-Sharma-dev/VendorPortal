namespace Server.Model.DTO.AuditDto
{
    public class AuditLogFilter
    {
        public string? Entity { get; set; }
        public Guid? EntityId { get; set; }
        public Guid? UserId { get; set; }
        public string? Username { get; set; }
        public string? Action { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
