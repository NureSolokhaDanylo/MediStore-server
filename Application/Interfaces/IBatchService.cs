using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IBatchService : IReadOnlyService<Batch>, ICreateService<Batch>, IUpdateService<Batch>, IDeleteService<Batch>
{
    Task<Result<(IEnumerable<Batch> items, int totalCount)>> SearchByBatchNumber(string userId, string query, int limit, int offset);
}
