using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    internal class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(AppDbContext context) : base(context) { }

        public async Task<List<AuditLog>> GetByTypeAsync(string entityType, DateTime? from = null, DateTime? to = null, int? take = null)
        {
            var query = _context.Set<AuditLog>().AsQueryable();

            query = query.Where(a => a.EntityType == entityType);

            if (from.HasValue)
                query = query.Where(a => a.OccurredAt >= from.Value);
            if (to.HasValue)
                query = query.Where(a => a.OccurredAt <= to.Value);

            query = query.OrderByDescending(a => a.OccurredAt);

            if (take.HasValue && take.Value > 0)
                query = query.Take(take.Value);

            return await query.ToListAsync();
        }
    }
}
