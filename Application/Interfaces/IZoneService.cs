using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IZoneService : ICrudService<Zone>
{
    Task<Result<(IEnumerable<Zone> items, int totalCount)>> Search(string userId, string query, int limit, int offset);
}
