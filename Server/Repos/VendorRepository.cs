using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Model;
using Server.Model.Enums;
using Server.Repos.Core;

namespace Server.Repos
{
    public interface IVendorRepository : IGenericRepository<Vendor>
    {
        Task<IEnumerable<Vendor>> GetByStatusAsync(VendorStatus status);
        Task<IEnumerable<Vendor>> GetByUserIdAsync(Guid userId);
        Task<Vendor> GetVendorWithDetailsAsync(Guid vendorId);
        Task<IEnumerable<Vendor>> GetAllWithDetailsAsync();
    }
    public class VendorRepository : GenericRepository<Vendor>, IVendorRepository
    {
        public VendorRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Vendor>> GetByStatusAsync(VendorStatus status)
        {
            return await _context.Vendors
                .Include(v => v.BankDetails)
                .Where(v => v.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vendor>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Vendors
                .Include(v => v.BankDetails)
                .Include(v => v.Documents)
                .Include(v => v.StatusHistories)
                .Where(v => v.CreatedByUserId == userId)
                .ToListAsync();
        }

        public async Task<Vendor> GetVendorWithDetailsAsync(Guid vendorId)
        {
            return await _context.Vendors
                .Include(v => v.BankDetails)
                .Include(v => v.Documents)
                .Include(v => v.StatusHistories)
                .FirstOrDefaultAsync(v => v.VendorId == vendorId);
        }

        public async Task<IEnumerable<Vendor>> GetAllWithDetailsAsync()
        {
            return await _context.Vendors
                .Include(v => v.BankDetails)
                .ToListAsync();
        }
    }
}
