namespace Server.Model.DTO.VendorsDto
{
    public class ReportFilter
    {
        public string? Status { get; set; }
        public string? VendorType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
