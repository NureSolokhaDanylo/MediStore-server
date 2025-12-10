using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;

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

        private readonly AppDbContext _context;

        public UnitOfWork(
            AppDbContext context,
            IMedicineRepository medicines,
            ISensorRepository sensors,
            IBatchRepository batches,
            IReadingRepository readings,
            IZoneRepository zones,
            IAlertRepository alerts,
            ISensorApiKeyRepository sensorApiKeys,
            IAppSettingsRepository appSettings)
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
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
