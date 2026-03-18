using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Model;
using Server.Repos.Core;

namespace Server.Repos
{
    public interface IVendorStatusHistoryRepository : IGenericRepository<VendorStatusHistory>
    {
        Task<IEnumerable<VendorStatusHistory>> GetByVendorIdAsync(Guid vendorId);
    }
    public class VendorStatusHistoryRepository : GenericRepository<VendorStatusHistory>, IVendorStatusHistoryRepository
    {
        public VendorStatusHistoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<VendorStatusHistory>> GetByVendorIdAsync(Guid vendorId)
        {
            return await _context.VendorStatusHistories
                .Where(v => v.VendorId == vendorId)
                .ToListAsync();
        }
    }
}
