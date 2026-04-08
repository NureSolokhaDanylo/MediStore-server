using System.Threading.Tasks;

using Domain.Models;

using Infrastructure.Interfaces;

namespace Infrastructure.UOW
{
    public interface IUnitOfWork
    {
        IMedicineRepository Medicines { get; }
        ISensorRepository Sensors { get; }
        IBatchRepository Batches { get; }
        IReadingRepository Readings { get; }
        IZoneRepository Zones { get; }
        IAlertRepository Alerts { get; }
        ISensorApiKeyRepository SensorApiKeys { get; }
        IAppSettingsRepository AppSettings { get; }
        IAuditLogRepository AuditLogs { get; }
        IUserDeviceRepository UserDevices { get; }

        IRepository<T> GetRepository<T>() where T : EntityBase;

        Task<int> SaveChangesAsync();
    }
}
