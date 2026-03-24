using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Domain.Enums;

namespace Infrastructure.Repositories
{
    internal class AlertRepository : Repository<Alert>, IAlertRepository
    {
        public AlertRepository(AppDbContext context) : base(context) { }

        public Task<bool> HasAlertForBatchAsync(int batchId, AlertType alertType)
        {
            return _context.Set<Alert>()
                .AnyAsync(a => a.BatchId == batchId && a.AlertType == alertType);
        }

        public Task<bool> HasAlertForZoneAsync(int zoneId, AlertType alertType)
        {
            return _context.Set<Alert>()
                .AnyAsync(a => a.ZoneId == zoneId && a.AlertType == alertType);
        }

        public Task<bool> HasActiveZoneConditionAlertAsync(int zoneId)
        {
            return _context.Set<Alert>()
                .AnyAsync(a => a.ZoneId == zoneId && a.AlertType == AlertType.ZoneConditionAlert && a.IsActive);
        }

        public Task<bool> HasActiveBatchConditionAlertAsync(int batchId)
        {
            return _context.Set<Alert>()
                .AnyAsync(a => a.BatchId == batchId && a.AlertType == AlertType.BatchConditionWarning && a.IsActive);
        }

        public Task<Alert?> GetActiveBatchConditionAlertAsync(int batchId)
        {
            return _context.Set<Alert>()
                .Where(a => a.BatchId == batchId && a.AlertType == AlertType.BatchConditionWarning && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public Task<Alert?> GetActiveZoneConditionAlertAsync(int zoneId)
        {
            return _context.Set<Alert>()
                .Where(a => a.ZoneId == zoneId && a.AlertType == AlertType.ZoneConditionAlert && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<(IEnumerable<Alert> Items, int TotalCount)> GetFilteredAlertsAsync(
            int skip, 
            int take, 
            bool? isActive = null, 
            int? zoneId = null, 
            int? batchId = null)
        {
            var query = _context.Set<Alert>().AsQueryable();

            // Apply filters
            if (isActive.HasValue)
                query = query.Where(a => a.IsActive == isActive.Value);

            if (zoneId.HasValue)
                query = query.Where(a => a.ZoneId == zoneId.Value);

            if (batchId.HasValue)
                query = query.Where(a => a.BatchId == batchId.Value);

            // Order by most recent first
            query = query.OrderByDescending(a => a.CreatedAt);

            // Get total count before applying pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
