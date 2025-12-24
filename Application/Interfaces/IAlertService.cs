using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IAlertService : IReadOnlyService<Alert>
{
    Task<Result> CreateZoneConditionAlertAsync(int zoneId, string message);
    Task<bool> HasActiveZoneConditionAlertAsync(int zoneId);
    Task<bool> HasActiveBatchConditionAlertAsync(int batchId);
}
