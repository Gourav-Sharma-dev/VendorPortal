using Server.Model.DTO.VendorsDto;
using Server.Repos;

namespace Server.Services
{
    public interface IReportService
    {
        Task<IEnumerable<VendorRegistrationReportDto>> GetVendorRegistrationReportAsync(ReportFilter filter);
        Task<IEnumerable<VendorVerificationReportDto>> GetVendorVerificationReportAsync(ReportFilter filter);
    }
    public class ReportService : IReportService
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly IVendorStatusHistoryRepository _statusHistoryRepository;

        public ReportService(
            IVendorRepository vendorRepository,
            IVendorStatusHistoryRepository statusHistoryRepository)
        {
            _vendorRepository = vendorRepository;
            _statusHistoryRepository = statusHistoryRepository;
        }

        public async Task<IEnumerable<VendorRegistrationReportDto>> GetVendorRegistrationReportAsync(ReportFilter filter)
        {
            var vendors = await _vendorRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(filter.Status))
                vendors = vendors.Where(v => v.Status.ToString() == filter.Status);

            if (!string.IsNullOrEmpty(filter.VendorType))
                vendors = vendors.Where(v => v.VendorType.ToString() == filter.VendorType);

            if (filter.FromDate.HasValue)
                vendors = vendors.Where(v => v.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                vendors = vendors.Where(v => v.CreatedAt <= filter.ToDate.Value);

            return vendors.Select(v => new VendorRegistrationReportDto
            {
                VendorCode = v.VendorCode,
                VendorName = v.VendorName,
                VendorType = v.VendorType.ToString(),
                Status = v.Status.ToString(),
                RegisteredDate = v.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            });
        }

        public async Task<IEnumerable<VendorVerificationReportDto>> GetVendorVerificationReportAsync(ReportFilter filter)
        {
            var vendors = await _vendorRepository.GetAllAsync();

            return vendors.Select(v => new VendorVerificationReportDto
            {
                VendorCode = v.VendorCode,
                VendorName = v.VendorName,
                Status = v.Status.ToString(),
                ProcurementApproved = v.ProcurementApproved,
                FinanceApproved = v.FinanceApproved,
                VerifiedBy = "N/A",
                VerifiedDate = v.UpdatedAt.ToString("yyyy-MM-dd") ?? "N/A"
            });
        }
    }
}
