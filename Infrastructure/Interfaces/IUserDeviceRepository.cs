using Domain.Models;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IUserDeviceRepository : IRepository<UserDevice>
    {
        Task<UserDevice?> GetByUserIdAsync(string userId);
    }
}
