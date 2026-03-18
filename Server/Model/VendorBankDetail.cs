using System.ComponentModel.DataAnnotations;

namespace Server.Model
{
    public class VendorBankDetail
    {
        [Key]
        public Guid BankDetailId { get; set; }
        public Guid VendorId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IFSCCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;

        public Vendor Vendor { get; set; } = null!;
    }
}
