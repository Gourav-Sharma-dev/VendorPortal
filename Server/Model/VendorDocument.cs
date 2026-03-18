using Server.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Server.Model
{
    public class VendorDocument
    {
        [Key]
        public Guid DocumentId { get; set; }
        public Guid VendorId { get; set; }
        public DocumentType DocumentType { get; set; }
        //public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileFormat { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public bool IsMandatory { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Vendor Vendor { get; set; } = null!;
    }
}
