namespace Server.Model.DTO.VendorsDto
{
    public class VendorRequest
    {
        public required string VendorName { get; set; }
        public required string VendorType { get; set; }
        public required string ContactPerson { get; set; }
        public required string MobileNumber { get; set; }
        public required string EmailAddress { get; set; }
        public required string CompanyName { get; set; }
        public required string CompanyAddress { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string Country { get; set; }
        public required string PinCode { get; set; }
        public required string GSTNumber { get; set; }
        public required string PANNumber { get; set; }
        public VendorBankDetailRequest? BankDetail { get; set; }
    }
}
