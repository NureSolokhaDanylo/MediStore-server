using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    internal class ZoneRepository : Repository<Zone>, IZoneRepository
    {
        public ZoneRepository(AppDbContext context) : base(context) { }

        public Task<List<Zone>> GetAllWithSensorsAsync()
        {
            return _context.Set<Zone>()
                .Include(z => z.Sensors)
                .ToListAsync();
        }

        public async Task<(List<Zone> items, int totalCount)> SearchAsync(string query, int limit, int offset)
        {
            var baseQuery = _set.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLower();
                baseQuery = baseQuery.Where(z => z.Name.ToLower().Contains(lowerQuery) || 
                            (z.Description != null && z.Description.ToLower().Contains(lowerQuery)));
            }

            var totalCount = await baseQuery.CountAsync();
            var items = await baseQuery
                .OrderBy(z => z.Name)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
