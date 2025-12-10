using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IReadingService : IReadOnlyService<Reading>
{
    Task<Result<Reading>> CreateForSensorAsync(int sensorId, Reading reading);
}
