using Domain.Models;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    internal class UserDeviceRepository : Repository<UserDevice>, IUserDeviceRepository
    {
        public UserDeviceRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<UserDevice?> GetByUserIdAsync(string userId)
        {
            return await _context.UserDevices
                .FirstOrDefaultAsync(ud => ud.UserId == userId);
        }
    }
}
