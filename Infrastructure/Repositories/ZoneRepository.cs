using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ZoneRepository : Repository<Zone>, IZoneRepository
    {
        public ZoneRepository(AppDbContext context) : base(context) { }

        public Task<List<Zone>> GetAllWithSensorsAsync()
        {
            return _context.Set<Zone>()
                .Include(z => z.Sensors)
                .ToListAsync();
        }
    }
}
