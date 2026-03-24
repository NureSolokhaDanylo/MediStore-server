using Domain.Models;
using System.Threading.Tasks;
using Domain.Enums;

namespace Infrastructure.Interfaces
{
    public interface IAlertRepository : IRepository<Alert>
    {
        Task<bool> HasAlertForBatchAsync(int batchId, AlertType alertType);
        Task<bool> HasAlertForZoneAsync(int zoneId, AlertType alertType);
        Task<bool> HasActiveZoneConditionAlertAsync(int zoneId);
        Task<bool> HasActiveBatchConditionAlertAsync(int batchId);
        Task<Alert?> GetActiveBatchConditionAlertAsync(int batchId);
        Task<Alert?> GetActiveZoneConditionAlertAsync(int zoneId);
        
        Task<(IEnumerable<Alert> Items, int TotalCount)> GetFilteredAlertsAsync(
            int skip, 
            int take, 
            bool? isActive = null, 
            int? zoneId = null, 
            int? batchId = null);
    }
}
