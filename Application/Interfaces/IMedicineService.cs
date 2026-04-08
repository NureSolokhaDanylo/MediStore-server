using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IMedicineService : IReadOnlyService<Medicine>, ICreateService<Medicine>, IUpdateService<Medicine>, IDeleteService<Medicine>
{
    Task<Result<(IEnumerable<Medicine> items, int totalCount)>> Search(string query, int limit, int offset);
}
