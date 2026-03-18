namespace Server.Model.DTO.VendorsDto
{
    public class VendorBankDetailRequest
    {
        public required string BankName { get; set; }
        public required string AccountNumber { get; set; }
        public required string IFSCCode { get; set; }
        public required string BranchName { get; set; }
    }
}
