using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    internal class MedicineRepository : Repository<Medicine>, IMedicineRepository
    {
        public MedicineRepository(AppDbContext context) : base(context) { }

        public async Task<(List<Medicine> items, int totalCount)> SearchAsync(string query, int limit, int offset)
        {
            var baseQuery = _set.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLower();
                baseQuery = baseQuery.Where(m => m.Name.ToLower().Contains(lowerQuery) || 
                            (m.Description != null && m.Description.ToLower().Contains(lowerQuery)));
            }

            var totalCount = await baseQuery.CountAsync();
            var items = await baseQuery
                .OrderBy(m => m.Name)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
