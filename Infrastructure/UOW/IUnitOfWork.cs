using System.Threading.Tasks;

using Infrastructure.Interfaces;

namespace Infrastructure.UOW
{
    public interface IUnitOfWork
    {
        //IUserRepository Users { get; }
        IMedicineRepository Medicines { get; }
        ISensorRepository Sensors { get; }
        IBatchRepository Batches { get; }
        IReadingRepository Readings { get; }
        IZoneRepository Zones { get; }
        IAlertRepository Alerts { get; }
        ISensorApiKeyRepository SensorApiKeys { get; }

        Task<int> SaveChangesAsync();
    }
}
