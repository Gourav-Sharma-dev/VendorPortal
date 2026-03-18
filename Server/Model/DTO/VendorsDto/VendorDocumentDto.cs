namespace Server.Model.DTO.VendorsDto
{
    public class VendorDocumentDto
    {
        public Guid DocumentId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileFormat { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public bool IsMandatory { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
