using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface ISensorService : IService<Sensor>
{
    Task<Result<Sensor>> UpdateFromDtoAsync(int id, string? serialNumber, bool? isOn, int? zoneId);
}
