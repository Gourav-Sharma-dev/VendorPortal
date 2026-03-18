namespace Server.Model.DTO.VendorsDto
{
    /// <summary>
    /// Data Transfer Object for Vendor information
    /// </summary>
    public class VendorDto
    {
        public Guid VendorId { get; set; }
        
        /// <summary>
        /// Vendor Code - Only visible when status is Approved or Active
        /// </summary>
        public string? VendorCode { get; set; }
        
        public string VendorName { get; set; } = string.Empty;
        public string VendorType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
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
        
        /// <summary>
        /// Bank details - Only included for authorized users
        /// </summary>
        public VendorBankDetailRequest? BankDetail { get; set; }
        
        /// <summary>
        /// Procurement approval status
        /// </summary>
        public bool? ProcurementApproved { get; set; }
        public string? ProcurementRemarks { get; set; }
        
        /// <summary>
        /// Finance approval status
        /// </summary>
        public bool? FinanceApproved { get; set; }
        public string? FinanceRemarks { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
