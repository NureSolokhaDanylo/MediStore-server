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
    }
}
