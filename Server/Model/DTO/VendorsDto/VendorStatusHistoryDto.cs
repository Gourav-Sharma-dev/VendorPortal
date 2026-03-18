namespace Server.Model.DTO.VendorsDto
{
    public class VendorStatusHistoryDto
    {
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime ActionDate { get; set; }
        public string ActionBy { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string? ApprovalLevel { get; set; }
    }
}
