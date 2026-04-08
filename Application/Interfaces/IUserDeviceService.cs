using Application.DTOs.UserDevice;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserDeviceService
    {
        Task<Result> RegisterDeviceAsync(CreateUserDeviceDto dto);
    }
}
