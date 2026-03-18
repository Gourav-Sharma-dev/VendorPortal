using System.ComponentModel.DataAnnotations;
using Server.Model.Enums;

namespace Server.Model
{
    public class Vendor
    {
        [Key]
        public Guid VendorId { get; set; }
        public string? VendorCode { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public VendorType VendorType { get; set; }
        public string ContactPerson { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PinCode { get; set; } = string.Empty;
        public string GSTNumber { get; set; } = string.Empty;
        public string PANNumber { get; set; } = string.Empty;
        public VendorStatus Status { get; set; }

        // Who registered this vendor
        public Guid? CreatedByUserId { get; set; }

        // Procurement Approval Tracking
        public bool ProcurementApproved { get; set; }
        public Guid? ProcurementApprovedBy { get; set; }
        public DateTime? ProcurementApprovedAt { get; set; }
        public string? ProcurementRemarks { get; set; }

        // Finance Approval Tracking
        public bool FinanceApproved { get; set; }
        public Guid? FinanceApprovedBy { get; set; }
        public DateTime? FinanceApprovedAt { get; set; }
        public string? FinanceRemarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<VendorBankDetail> BankDetails { get; set; } = new List<VendorBankDetail>();
        public ICollection<VendorDocument> Documents { get; set; } = new List<VendorDocument>();
        public ICollection<VendorStatusHistory> StatusHistories { get; set; } = new List<VendorStatusHistory>();
    }
}
