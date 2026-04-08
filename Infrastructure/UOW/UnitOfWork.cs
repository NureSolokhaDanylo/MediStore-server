using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.Repositories;

namespace Infrastructure.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        public IMedicineRepository Medicines { get; }
        public ISensorRepository Sensors { get; }
        public IBatchRepository Batches { get; }
        public IReadingRepository Readings { get; }
        public IZoneRepository Zones { get; }
        public IAlertRepository Alerts { get; }
        public ISensorApiKeyRepository SensorApiKeys { get; }
        public IAppSettingsRepository AppSettings { get; }
        public IAuditLogRepository AuditLogs { get; }
        public IUserDeviceRepository UserDevices { get; }

        private readonly AppDbContext _context;

        internal UnitOfWork(
            AppDbContext context,
            IMedicineRepository medicines,
            ISensorRepository sensors,
            IBatchRepository batches,
            IReadingRepository readings,
            IZoneRepository zones,
            IAlertRepository alerts,
            ISensorApiKeyRepository sensorApiKeys,
            IAppSettingsRepository appSettings,
            IAuditLogRepository auditLogs,
            IUserDeviceRepository userDevices)
        {
            _context = context;

            Medicines = medicines;
            Sensors = sensors;
            Batches = batches;
            Readings = readings;
            Zones = zones;
            Alerts = alerts;
            SensorApiKeys = sensorApiKeys;
            AppSettings = appSettings;
            AuditLogs = auditLogs;
            UserDevices = userDevices;
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

        public IRepository<T> GetRepository<T>() where T : EntityBase
        {
            return typeof(T).Name switch
            {
                nameof(Medicine) => (IRepository<T>)Medicines,
                nameof(Sensor) => (IRepository<T>)Sensors,
                nameof(Batch) => (IRepository<T>)Batches,
                nameof(Reading) => (IRepository<T>)Readings,
                nameof(Zone) => (IRepository<T>)Zones,
                nameof(Alert) => (IRepository<T>)Alerts,
                nameof(SensorApiKey) => (IRepository<T>)SensorApiKeys,
                nameof(AuditLog) => (IRepository<T>)AuditLogs,
                nameof(UserDevice) => (IRepository<T>)UserDevices,
                _ => throw new InvalidOperationException($"No repository registered in unit of work for entity type {typeof(T).FullName}.")
            };
        }
    }
}
