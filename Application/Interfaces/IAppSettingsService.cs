using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAppSettingsService
    {
        Task<Result<AppSettings>> GetSingletonAsync();
        Task<Result<AppSettings>> Update(AppSettings entity);
    }
}
