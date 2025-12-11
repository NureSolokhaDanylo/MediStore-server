using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IAlertService : ICrudService<Alert>
{
    Task<Result> CreateZoneConditionAlertAsync(int zoneId, int sensorId, string message);
}
