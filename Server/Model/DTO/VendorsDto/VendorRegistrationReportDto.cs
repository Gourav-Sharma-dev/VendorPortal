namespace Server.Model.DTO.VendorsDto
{
    public class VendorRegistrationReportDto
    {
        public required string VendorCode { get; set; }
        public required string VendorName { get; set; }
        public required string VendorType { get; set; }
        public required string Status { get; set; }
        public required string RegisteredDate { get; set; }
    }
}
