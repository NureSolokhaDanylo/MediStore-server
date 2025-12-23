using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    internal class AlertRepository : Repository<Alert>, IAlertRepository
    {
        public AlertRepository(AppDbContext context) : base(context) { }

        public Task<bool> HasAlertForBatchAsync(int batchId, Domain.Enums.AlertType alertType)
        {
            return _context.Set<Alert>()
                .AnyAsync(a => a.BatchId == batchId && a.AlertType == alertType);
        }

        public Task<bool> HasAlertForZoneAsync(int zoneId, Domain.Enums.AlertType alertType)
        {
            return _context.Set<Alert>()
                .AnyAsync(a => a.ZoneId == zoneId && a.AlertType == alertType);
        }
    }
}
