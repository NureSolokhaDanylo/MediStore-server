using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    internal class BatchRepository : Repository<Batch>, IBatchRepository
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

        public async Task<(List<Batch> items, int totalCount)> SearchByBatchNumberAsync(string query, int limit, int offset)
        {
            var baseQuery = _set.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLower();
                baseQuery = baseQuery.Where(b => b.BatchNumber.ToLower().Contains(lowerQuery));
            }

            var totalCount = await baseQuery.CountAsync();
            var items = await baseQuery
                .OrderBy(b => b.BatchNumber)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
