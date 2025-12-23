using Application.Interfaces;
using Application.Results.Base;

using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class AlertService : CrudService<Alert>, IAlertService
{
    private readonly IAlertRepository _alertRepo;

    public AlertService(IAlertRepository repository, IUnitOfWork uow) : base(repository, uow)
    {
        _alertRepo = repository;
    }

    public async Task<Result> CreateZoneConditionAlertAsync(int zoneId, int sensorId, string message)
    {
        var alert = new Alert
        {
            ZoneId = zoneId,
            SensorId = sensorId,
            AlertType = Domain.Enums.AlertType.ZoneConditionAlert,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Message = message
        };

        await _alertRepo.AddAsync(alert);
        await _uow.SaveChangesAsync();

        return Result.Success();
    }
}
