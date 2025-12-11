using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BatchRepository : Repository<Batch>, IBatchRepository
    {
        public BatchRepository(AppDbContext context) : base(context) { }

        public Task<List<Batch>> GetExpiredBatchesAsync(DateTime asOf)
        {
            return _context.Set<Batch>()
                .Where(b => b.ExpireDate <= asOf)
                .ToListAsync();
        }

        public Task<List<Batch>> GetBatchesApproachingExpirationAsync(DateTime now)
        {
            // We'll return batches where ExpireDate >= now (not yet expired).
            // Further filtering by medicine.WarningThresholdDays will be done in service layer.
            return _context.Set<Batch>()
                .Where(b => b.ExpireDate >= now)
                .ToListAsync();
        }
    }
}
