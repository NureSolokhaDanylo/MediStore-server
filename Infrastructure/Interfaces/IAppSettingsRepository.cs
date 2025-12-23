using Domain.Models;

namespace Infrastructure.Interfaces
{
    public interface IAppSettingsRepository
    {
        Task<AppSettings?> GetAsync();
        Task UpdateAsync(AppSettings appSettings);
        Task AddAsync(AppSettings appSettings);
    }
}
