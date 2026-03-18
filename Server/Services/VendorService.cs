using Server.Model;
using Server.Model.DTO.VendorsDto;
using Server.Model.Enums;
using Server.Repos;
using Server.Repos.Core;
using Server.Services.Common;

namespace Server.Services
{
    public interface IVendorService
    {
        Task<VendorDto> GetVendorByIdAsync(Guid vendorId);
        Task<IEnumerable<VendorDto>> GetAllVendorsAsync();
        Task<IEnumerable<VendorDto>> GetVendorsByStatusAsync(VendorStatus status);
        Task<IEnumerable<VendorDto>> GetMyVendorsAsync(string userId);
        Task<Guid> RegisterVendorAsync(string userId, VendorRequest request);
        Task<bool> UpdateVendorAsync(string userId, Guid vendorId, VendorRequest request);
        Task<bool> UploadDocumentAsync(string userId, Guid vendorId, string documentType, IFormFile file, string uploadPath);
        Task<IEnumerable<VendorDocumentDto>> GetVendorDocumentsAsync(Guid vendorId);
    }

    public class VendorService : IVendorService
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly IGenericRepository<VendorBankDetail> _bankDetailRepository;
        private readonly IGenericRepository<VendorDocument> _documentRepository;
        private readonly IAuditService _auditService;
        private readonly ICryptoService _cryptoService;

        public VendorService(
            IVendorRepository vendorRepository,
            IGenericRepository<VendorBankDetail> bankDetailRepository,
            IGenericRepository<VendorDocument> documentRepository,
            IAuditService auditService,
            ICryptoService cryptoService)
        {
            _vendorRepository     = vendorRepository;
            _bankDetailRepository = bankDetailRepository;
            _documentRepository   = documentRepository;
            _auditService         = auditService;
            _cryptoService        = cryptoService;
        }

        public async Task<VendorDto> GetVendorByIdAsync(Guid vendorId)
        {
            var vendor = await _vendorRepository.GetVendorWithDetailsAsync(vendorId);
            if (vendor == null) return null;

            return MapToDto(vendor);
        }

        public async Task<IEnumerable<VendorDto>> GetAllVendorsAsync()
        {
            var vendors = await _vendorRepository.GetAllWithDetailsAsync();
            return vendors.Select(v => MapToDto(v));
        }

        public async Task<IEnumerable<VendorDto>> GetMyVendorsAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return [];
            var vendors = await _vendorRepository.GetByUserIdAsync(userGuid);
            return vendors.Select(v => MapToDto(v));
        }

        public async Task<IEnumerable<VendorDto>> GetVendorsByStatusAsync(VendorStatus status)
        {
            var vendors = await _vendorRepository.GetByStatusAsync(status);
            return vendors.Select(v => MapToDto(v));
        }

        public async Task<Guid> RegisterVendorAsync(string userId, VendorRequest request)
        {
            var vendor = new Vendor
            {
                VendorId = Guid.NewGuid(),
                VendorCode = GenerateVendorCode(),
                VendorName = request.VendorName,
                VendorType = Enum.Parse<VendorType>(request.VendorType),
                ContactPerson = request.ContactPerson,
                MobileNumber = request.MobileNumber,
                EmailAddress = request.EmailAddress,
                CompanyName = request.CompanyName,
                CompanyAddress = request.CompanyAddress,
                City = request.City,
                State = request.State,
                Country = request.Country,
                PinCode = request.PinCode,
                GSTNumber = request.GSTNumber,
                PANNumber = request.PANNumber,
                Status = VendorStatus.Submitted,
                CreatedByUserId = Guid.TryParse(userId, out var uid) ? uid : (Guid?)null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _vendorRepository.AddAsync(vendor);

            if (request.BankDetail != null)
            {
                var bankDetail = new VendorBankDetail
                {
                    BankDetailId = Guid.NewGuid(),
                    VendorId = vendor.VendorId,
                    BankName = request.BankDetail.BankName,
                    AccountNumber = _cryptoService.Encrypt(request.BankDetail.AccountNumber),
                    IFSCCode = _cryptoService.Encrypt(request.BankDetail.IFSCCode),
                    BranchName = request.BankDetail.BranchName
                };
                await _bankDetailRepository.AddAsync(bankDetail);
            }

            await _vendorRepository.SaveChangesAsync();

            await _auditService.LogAsync(
                Guid.Parse(userId), 
                "VendorRegistration", 
                "Vendor", 
                vendor.VendorId, 
                $"Vendor '{vendor.VendorName}' registered with code {vendor.VendorCode}");

            return vendor.VendorId;
        }

        public async Task<bool> UpdateVendorAsync(string userId, Guid vendorId, VendorRequest request)
        {
            var vendor = await _vendorRepository.GetByIdAsync(vendorId);
            if (vendor == null) return false;

            // Prevent editing if vendor is beyond certain statuses
            if (vendor.Status == VendorStatus.Approved || 
                vendor.Status == VendorStatus.Active || 
                vendor.Status == VendorStatus.Rejected)
            {
                return false;
            }

            vendor.VendorName = request.VendorName;
            vendor.VendorType = Enum.Parse<VendorType>(request.VendorType);
            vendor.ContactPerson = request.ContactPerson;
            vendor.MobileNumber = request.MobileNumber;
            vendor.EmailAddress = request.EmailAddress;
            vendor.CompanyName = request.CompanyName;
            vendor.CompanyAddress = request.CompanyAddress;
            vendor.City = request.City;
            vendor.State = request.State;
            vendor.Country = request.Country;
            vendor.PinCode = request.PinCode;
            vendor.GSTNumber = request.GSTNumber;
            vendor.PANNumber = request.PANNumber;
            vendor.UpdatedAt = DateTime.UtcNow;

            if (request.BankDetail != null)
            {
                var bankDetail = (await _bankDetailRepository.GetAllAsync())
                    .FirstOrDefault(x => x.VendorId == vendorId);

                if (bankDetail != null)
                {
                    bankDetail.BankName = request.BankDetail.BankName;
                    bankDetail.AccountNumber = _cryptoService.Encrypt(request.BankDetail.AccountNumber);
                    bankDetail.IFSCCode = _cryptoService.Encrypt(request.BankDetail.IFSCCode);
                    bankDetail.BranchName = request.BankDetail.BranchName;
                    await _bankDetailRepository.UpdateAsync(bankDetail);
                }
                else
                {
                    var newBankDetail = new VendorBankDetail
                    {
                        BankDetailId = Guid.NewGuid(),
                        VendorId = vendorId,
                        BankName = request.BankDetail.BankName,
                        AccountNumber = _cryptoService.Encrypt(request.BankDetail.AccountNumber),
                        IFSCCode = _cryptoService.Encrypt(request.BankDetail.IFSCCode),
                        BranchName = request.BankDetail.BranchName
                    };
                    await _bankDetailRepository.AddAsync(newBankDetail);
                }
            }

            // Reset approval status to Submitted after update
            if (vendor.Status == VendorStatus.CorrectionRequested)
            {
                vendor.Status = VendorStatus.Submitted;
            }

            await _vendorRepository.UpdateAsync(vendor);
            await _vendorRepository.SaveChangesAsync();
            await _auditService.LogAsync(
                Guid.Parse(userId), 
                "VendorUpdate", 
                "Vendor", 
                vendorId, 
                $"Vendor '{vendor.VendorName}' updated");

            return true;
        }

        public async Task<bool> UploadDocumentAsync(string userId, Guid vendorId, string documentType, IFormFile file,string uploadPath)
        {
            var vendor = await _vendorRepository.GetByIdAsync(vendorId);
            if (vendor == null) return false;

            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return false;

            if (!Enum.TryParse<DocumentType>(documentType, true, out var docType))
                return false;

            // Generate unique filename
            // Extract only the last part of the vendorId Guid
            var vendorIdLastPart = vendorId.ToString().Split('-').Last();
            var fileName = $"{vendorIdLastPart}_{docType}_{DateTime.Now:ddMMyyyyhhmm}{extension}";
            //var fileName = $"{vendorId}_{docType}_{DateTime.Now.ToString("ddMMyyyyhhmm")}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save document metadata
            var document = new VendorDocument
            {
                DocumentId = Guid.NewGuid(),
                VendorId = vendorId,
                DocumentType = docType,
                FileName = fileName,
                FileFormat = extension,
                FileSize = file.Length,
                IsMandatory = docType == DocumentType.GSTCertificate || docType == DocumentType.PANCard,
                UploadedAt = DateTime.UtcNow
            };

            await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();
            await _auditService.LogAsync(
                Guid.Parse(userId), 
                "DocumentUpload", 
                "VendorDocument", 
                vendorId, 
                $"Document '{docType}' uploaded for vendor");

            return true;
        }

        public async Task<IEnumerable<VendorDocumentDto>> GetVendorDocumentsAsync(Guid vendorId)
        {
            var documents = (await _documentRepository.GetAllAsync())
                .Where(d => d.VendorId == vendorId)
                .ToList();

            return documents.Select(d => new VendorDocumentDto
            {
                DocumentId   = d.DocumentId,
                DocumentType = d.DocumentType.ToString(),
                FileName     = d.FileName,
                FileFormat   = d.FileFormat,
                FileSize     = d.FileSize,
                IsMandatory  = d.IsMandatory,
                UploadedAt   = d.UploadedAt
            });
        }

        private VendorDto MapToDto(Vendor vendor)
        {
            var bankDetailEntity = vendor.BankDetails?.FirstOrDefault();
            VendorBankDetailRequest? bankDetailDto = null;

            if (bankDetailEntity != null)
            {
                bankDetailDto = new VendorBankDetailRequest
                {
                    BankName = bankDetailEntity.BankName,
                    AccountNumber = _cryptoService.Decrypt(bankDetailEntity.AccountNumber),
                    IFSCCode = _cryptoService.Decrypt(bankDetailEntity.IFSCCode),
                    BranchName = bankDetailEntity.BranchName
                };
            }

            return new VendorDto
            {
                VendorId = vendor.VendorId,
                // Only show vendor code if approved or active
                VendorCode = (vendor.Status == VendorStatus.Approved || vendor.Status == VendorStatus.Active) 
                    ? vendor.VendorCode 
                    : null,
                VendorName = vendor.VendorName,
                VendorType = vendor.VendorType.ToString(),
                Status = vendor.Status.ToString(),
                ContactPerson = vendor.ContactPerson,
                MobileNumber = vendor.MobileNumber,
                EmailAddress = vendor.EmailAddress,
                CompanyName = vendor.CompanyName,
                CompanyAddress = vendor.CompanyAddress,
                City = vendor.City,
                State = vendor.State,
                Country = vendor.Country,
                PinCode = vendor.PinCode,
                GSTNumber = vendor.GSTNumber,
                PANNumber = vendor.PANNumber,
                BankDetail = bankDetailDto,
                ProcurementApproved = vendor.ProcurementApproved,
                ProcurementRemarks = vendor.ProcurementRemarks,
                FinanceApproved = vendor.FinanceApproved,
                FinanceRemarks = vendor.FinanceRemarks,
                CreatedAt = vendor.CreatedAt,
                UpdatedAt = vendor.UpdatedAt
            };
        }

        private string GenerateVendorCode()
        {
            return "VEN-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "-" + Guid.NewGuid().ToString("N")[..6].ToUpper();
        }
    }
}
