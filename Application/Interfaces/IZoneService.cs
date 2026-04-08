using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IZoneService : IReadOnlyService<Zone>, ICreateService<Zone>, IUpdateService<Zone>, IDeleteService<Zone>
{
    Task<Result<(IEnumerable<Zone> items, int totalCount)>> Search(string query, int limit, int offset);
}
