using Domain.Models;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IZoneRepository : IRepository<Zone>
    {
        Task<List<Zone>> GetAllWithSensorsAsync();
    }
}
