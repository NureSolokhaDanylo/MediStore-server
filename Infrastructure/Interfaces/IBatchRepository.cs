using Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IBatchRepository : IRepository<Batch>
    {
        Task<List<Batch>> GetExpiredBatchesAsync(DateTime asOf);
        Task<List<Batch>> GetBatchesApproachingExpirationAsync(DateTime now);
    }
}
