using Server.Model;
using Server.Model.DTO.VendorsDto;
using Server.Model.Enums;
using Server.Repos;
using Server.Repos.Core;

namespace Server.Services
{
    public interface IApprovalService
    {
        Task<bool> ProcurementApprovalAsync(Guid vendorId, Guid approvedBy, bool isApproved, string? remarks);
        Task<bool> FinanceApprovalAsync(Guid vendorId, Guid approvedBy, bool isApproved, string? remarks);
        Task<bool> RejectVendorAsync(Guid vendorId, Guid rejectedBy, string remarks);
        Task<bool> RequestCorrectionAsync(Guid vendorId, Guid requestedBy, string remarks);
        Task<bool> ActivateVendorAsync(Guid vendorId, Guid activatedBy);
        Task<IEnumerable<VendorStatusHistoryDto>> GetVendorStatusHistoryAsync(Guid vendorId);
    }

    public class ApprovalService : IApprovalService
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly IGenericRepository<VendorStatusHistory> _statusHistoryRepository;
        private readonly IAuditService _auditService;

        public ApprovalService(
            IVendorRepository vendorRepository,
            IGenericRepository<VendorStatusHistory> statusHistoryRepository,
            IAuditService auditService)
        {
            _vendorRepository = vendorRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _auditService = auditService;
        }

        public async Task<bool> ProcurementApprovalAsync(Guid vendorId, Guid approvedBy, bool isApproved, string? remarks)
        {
            var vendor = await _vendorRepository.GetByIdAsync(vendorId);
            if (vendor == null) return false;

            if (vendor.Status != VendorStatus.Submitted && vendor.Status != VendorStatus.UnderReview)
                return false;

            var oldStatus = vendor.Status;
            vendor.ProcurementApproved = isApproved;
            vendor.ProcurementApprovedBy = approvedBy;
            vendor.ProcurementApprovedAt = DateTime.UtcNow;
            vendor.ProcurementRemarks = remarks;
            vendor.UpdatedAt = DateTime.UtcNow;

            if (isApproved)
            {
                vendor.Status = VendorStatus.FinanceVerification;
                await LogStatusChange(vendorId, oldStatus, VendorStatus.FinanceVerification, approvedBy, remarks, ApprovalLevel.Procurement);
                await _auditService.LogAsync(approvedBy, "ProcurementApproved", "Vendor", vendorId, 
                    $"Vendor {vendor.VendorName} approved by Procurement team. {remarks}");
            }
            else
            {
                vendor.Status = VendorStatus.CorrectionRequested;
                await LogStatusChange(vendorId, oldStatus, VendorStatus.CorrectionRequested, approvedBy, remarks, ApprovalLevel.Procurement);
                await _auditService.LogAsync(approvedBy, "ProcurementRejected", "Vendor", vendorId,
                    $"Vendor {vendor.VendorName} sent back for correction by Procurement team. {remarks}");
            }

            await _vendorRepository.UpdateAsync(vendor);
            await _vendorRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FinanceApprovalAsync(Guid vendorId, Guid approvedBy, bool isApproved, string? remarks)
        {
            var vendor = await _vendorRepository.GetByIdAsync(vendorId);
            if (vendor == null) return false;

            if (vendor.Status != VendorStatus.FinanceVerification)
                return false;

            if (vendor.ProcurementApproved != true)
                return false;

            var oldStatus = vendor.Status;
            vendor.FinanceApproved = isApproved;
            vendor.FinanceApprovedBy = approvedBy;
            vendor.FinanceApprovedAt = DateTime.UtcNow;
            vendor.FinanceRemarks = remarks;
            vendor.UpdatedAt = DateTime.UtcNow;

            if (isApproved)
            {
                vendor.Status = VendorStatus.Approved;
                await LogStatusChange(vendorId, oldStatus, VendorStatus.Approved, approvedBy, remarks, ApprovalLevel.Finance);
                await _auditService.LogAsync(approvedBy, "FinanceApproved", "Vendor", vendorId,
                    $"Vendor {vendor.VendorName} approved by Finance team. {remarks}");
            }
            else
            {
                vendor.Status = VendorStatus.CorrectionRequested;
                await LogStatusChange(vendorId, oldStatus, VendorStatus.CorrectionRequested, approvedBy, remarks, ApprovalLevel.Finance);
                await _auditService.LogAsync(approvedBy, "FinanceRejected", "Vendor", vendorId,
                    $"Vendor {vendor.VendorName} sent back for correction by Finance team. {remarks}");
            }

            await _vendorRepository.UpdateAsync(vendor);
            await _vendorRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectVendorAsync(Guid vendorId, Guid rejectedBy, string remarks)
        {
            var vendor = await _vendorRepository.GetByIdAsync(vendorId);
            if (vendor == null) return false;

            var oldStatus = vendor.Status;
            vendor.Status = VendorStatus.Rejected;
            vendor.UpdatedAt = DateTime.UtcNow;

            await _vendorRepository.UpdateAsync(vendor);
            await LogStatusChange(vendorId, oldStatus, VendorStatus.Rejected, rejectedBy, remarks, null);
            await _auditService.LogAsync(rejectedBy, "VendorRejected", "Vendor", vendorId,
                $"Vendor {vendor.VendorName} rejected. {remarks}");
            await _vendorRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RequestCorrectionAsync(Guid vendorId, Guid requestedBy, string remarks)
        {
            var vendor = await _vendorRepository.GetByIdAsync(vendorId);
            if (vendor == null) return false;

            var oldStatus = vendor.Status;
            vendor.Status = VendorStatus.CorrectionRequested;
            vendor.UpdatedAt = DateTime.UtcNow;

            await _vendorRepository.UpdateAsync(vendor);
            await LogStatusChange(vendorId, oldStatus, VendorStatus.CorrectionRequested, requestedBy, remarks, null);
            await _auditService.LogAsync(requestedBy, "CorrectionRequested", "Vendor", vendorId,
                $"Correction requested for vendor {vendor.VendorName}. {remarks}");
            await _vendorRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateVendorAsync(Guid vendorId, Guid activatedBy)
        {
            var vendor = await _vendorRepository.GetByIdAsync(vendorId);
            if (vendor == null) return false;

            if (vendor.Status != VendorStatus.Approved)
                return false;

            var oldStatus = vendor.Status;
            vendor.Status = VendorStatus.Active;
            vendor.UpdatedAt = DateTime.UtcNow;

            await _vendorRepository.UpdateAsync(vendor);
            await LogStatusChange(vendorId, oldStatus, VendorStatus.Active, activatedBy, "Vendor activated in system", null);
            await _auditService.LogAsync(activatedBy, "VendorActivated", "Vendor", vendorId,
                $"Vendor {vendor.VendorName} activated in system with code {vendor.VendorCode}");
            await _vendorRepository.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<VendorStatusHistoryDto>> GetVendorStatusHistoryAsync(Guid vendorId)
        {
            var histories = (await _statusHistoryRepository.GetAllAsync())
                .Where(h => h.VendorId == vendorId)
                .OrderByDescending(h => h.ActionDate)
                .ToList();

            return histories.Select(h => new VendorStatusHistoryDto
            {
                OldStatus = h.OldStatus.ToString(),
                NewStatus = h.NewStatus.ToString(),
                ActionDate = h.ActionDate,
                ActionBy = h.ActionBy.ToString(),
                Remarks = h.Remarks,
                ApprovalLevel = h.ApprovalLevel?.ToString()
            });
        }

        private async Task LogStatusChange(Guid vendorId, VendorStatus oldStatus, VendorStatus newStatus, 
            Guid actionBy, string? remarks, ApprovalLevel? approvalLevel)
        {
            var history = new VendorStatusHistory
            {
                StatusHistoryId = Guid.NewGuid(),
                VendorId = vendorId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ActionBy = actionBy,
                ActionDate = DateTime.UtcNow,
                Remarks = remarks,
                ApprovalLevel = approvalLevel
            };

            await _statusHistoryRepository.AddAsync(history);
        }
    }
}
