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
        //public IUserRepository Users { get; }
        public IMedicineRepository Medicines { get; }
        public ISensorRepository Sensors { get; }
        public IBatchRepository Batches { get; }
        public IReadingRepository Readings { get; }
        public IZoneRepository Zones { get; }
        public IAlertRepository Alerts { get; }

        private readonly AppDbContext _context;

        public UnitOfWork(
            AppDbContext context,
            //IUserRepository users,
            IMedicineRepository medicines,
            ISensorRepository sensors,
            IBatchRepository batches,
            IReadingRepository readings,
            IZoneRepository zones,
            IAlertRepository alerts)
        {
            _context = context;

            //Users = users;
            Medicines = medicines;
            Sensors = sensors;
            Batches = batches;
            Readings = readings;
            Zones = zones;
            Alerts = alerts;
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
