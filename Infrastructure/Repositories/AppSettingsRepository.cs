using Domain.Models;
using Infrastructure.Interfaces;

namespace Infrastructure.Repositories
{
    public class AppSettingsRepository : Repository<AppSettings>, IAppSettingsRepository
    {
        public AppSettingsRepository(AppDbContext context) : base(context) { }
    }
}
