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

        public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetByTypePagedAsync(
            string entityType,
            DateTime? from = null,
            DateTime? to = null,
            int skip = 0,
            int take = 50)
        {
            return await GetPagedAsync(null, entityType, null, null, from, to, skip, take);
        }

        public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(
            string? q = null,
            string? entityType = null,
            string? action = null,
            string? userId = null,
            DateTime? from = null,
            DateTime? to = null,
            int skip = 0,
            int take = 50)
        {
            var query = _context.Set<AuditLog>().AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var search = q.Trim();
                query = query.Where(a => a.Summary != null && a.Summary.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(entityType))
            {
                query = query.Where(a => a.EntityType == entityType);
            }

            if (!string.IsNullOrWhiteSpace(action))
            {
                query = query.Where(a => a.Action == action);
            }

            if (!string.IsNullOrWhiteSpace(userId))
            {
                query = query.Where(a => a.UserId == userId);
            }

            if (from.HasValue)
            {
                query = query.Where(a => a.OccurredAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(a => a.OccurredAt <= to.Value);
            }

            query = query.OrderByDescending(a => a.OccurredAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
