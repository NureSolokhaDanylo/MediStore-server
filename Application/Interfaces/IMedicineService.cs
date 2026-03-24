using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IMedicineService : ICrudService<Medicine>
{
    Task<Result<(IEnumerable<Medicine> items, int totalCount)>> Search(string userId, string query, int limit, int offset);
}
