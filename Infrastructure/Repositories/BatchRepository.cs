using Domain.Models;

using Infrastructure.Interfaces;

namespace Infrastructure.Repositories
{
    public class BatchRepository : Repository<Batch>, IBatchRepository
    {
        public BatchRepository(AppDbContext context) : base(context) { }
    }
}
