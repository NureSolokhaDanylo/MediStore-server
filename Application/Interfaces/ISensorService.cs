using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface ISensorService : IReadOnlyService<Sensor>
{
    Task<Result<Sensor>> UpdateFromAdmin(int id, string? serialNumber, bool? isOn, int? zoneId);

    Task<Result<Sensor>> Add(Sensor entity);
    Task<Result> Delete(int id);
}
