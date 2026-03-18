using Server.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Server.Model
{
    public class VendorStatusHistory
    {
        [Key]
        public Guid StatusHistoryId { get; set; }
        public Guid VendorId { get; set; }
        public VendorStatus OldStatus { get; set; }
        public VendorStatus NewStatus { get; set; }
        public Guid ActionBy { get; set; }
        public DateTime ActionDate { get; set; }
        public string? Remarks { get; set; } = string.Empty;
        public ApprovalLevel? ApprovalLevel { get; set; }

        public Vendor Vendor { get; set; } = null!;
        public User ActionUser { get; set; } = null!;
    }
}
