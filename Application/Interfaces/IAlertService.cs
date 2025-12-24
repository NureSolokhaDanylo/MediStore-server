using Domain.Enums;
using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IAlertService : IReadOnlyService<Alert>
{
    Task<Result> CreateZoneConditionAlertAsync(int zoneId, string message);
    Task<bool> HasActiveZoneConditionAlertAsync(int zoneId);
    Task<bool> HasActiveBatchConditionAlertAsync(int batchId);

    Task<Alert?> GetActiveZoneConditionAlertAsync(int zoneId);
    Task<Alert?> GetActiveBatchConditionAlertAsync(int batchId);

    Task<Result> AppendToZoneConditionAlertAsync(Alert alert, string message);
    Task<Result> ResolveZoneConditionAlertAsync(Alert alert);

    Task<Result> CreateBatchConditionAlertAsync(int batchId, int zoneId, string message);
    Task<Result> AppendToBatchConditionAlertAsync(Alert alert, string message);
    Task<Result> ResolveBatchConditionAlertAsync(Alert alert);

    Task<bool> HasAlertForBatchAsync(int batchId, AlertType alertType);
    Task<Result> CreateBatchAlertAsync(int batchId, AlertType alertType, string message);
}
