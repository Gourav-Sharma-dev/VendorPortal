namespace Server.Model.DTO.VendorsDto
{
    public class VendorVerificationReportDto
    {
        public required string VendorCode { get; set; }
        public required string VendorName { get; set; }
        public required bool ProcurementApproved { get; set; }
        public required bool FinanceApproved { get; set; }
        public required string Status { get; set; }
        public required string VerifiedBy { get; set; }
        public required string VerifiedDate { get; set; }
    }
}
