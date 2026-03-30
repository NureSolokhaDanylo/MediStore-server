using Domain.Enums;
using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface ISensorService : IReadOnlyService<Sensor>
{
    Task<Result<Sensor>> UpdateFromAdmin(int id, string? serialNumber, bool? isOn, int? zoneId);

    Task<Result<Sensor>> Add(Sensor entity);
    Task<Result> Delete(int id);
    
    Task<Result<IEnumerable<Sensor>>> GetByZoneIdAsync(string userId, int zoneId);
    Task<Result<(IEnumerable<Sensor> Items, int TotalCount)>> GetPagedAsync(
        string userId,
        int skip,
        int take,
        string? q = null,
        SensorType? sensorType = null,
        bool? isOn = null,
        int? zoneId = null);
}
