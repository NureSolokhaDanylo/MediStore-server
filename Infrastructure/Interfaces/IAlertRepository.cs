using Domain.Models;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IAlertRepository : IRepository<Alert>
    {
        Task<bool> HasAlertForBatchAsync(int batchId, Domain.Enums.AlertType alertType);
        Task<bool> HasAlertForZoneAsync(int zoneId, Domain.Enums.AlertType alertType);
        Task<bool> HasActiveZoneConditionAlertAsync(int zoneId);
        Task<bool> HasActiveBatchConditionAlertAsync(int batchId);
        Task<Alert?> GetActiveBatchConditionAlertAsync(int batchId);
        Task<Alert?> GetActiveZoneConditionAlertAsync(int zoneId);
    }
}
