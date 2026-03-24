using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IBatchService : ICrudService<Batch>
{
    Task<Result<(IEnumerable<Batch> items, int totalCount)>> SearchByBatchNumber(string userId, string query, int limit, int offset);
}
