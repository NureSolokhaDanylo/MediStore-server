using Domain.Models;

using Infrastructure.Interfaces;

namespace Infrastructure.Repositories
{
    public class ReadingRepository : Repository<Reading>, IReadingRepository
    {
        public ReadingRepository(AppDbContext context) : base(context) { }
    }
}
