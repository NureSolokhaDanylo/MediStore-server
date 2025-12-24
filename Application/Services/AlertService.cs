using Application.Interfaces;
using Application.Results.Base;

using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class AlertService : ReadOnlyService<Alert>, IAlertService
{
    private readonly IAlertRepository _alertRepo;

    public AlertService(IAlertRepository repository, IUnitOfWork uow) : base(repository, uow)
    {
        _alertRepo = repository;
    }

    public async Task<Result> CreateZoneConditionAlertAsync(int zoneId, string message)
    {
        var alert = new Alert
        {
            ZoneId = zoneId,
            AlertType = Domain.Enums.AlertType.ZoneConditionAlert,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Message = message
        };

        await _alertRepo.AddAsync(alert);
        await _uow.SaveChangesAsync();

        return Result.Success();
    }

    public Task<bool> HasActiveZoneConditionAlertAsync(int zoneId)
        => _alertRepo.HasActiveZoneConditionAlertAsync(zoneId);

    public Task<bool> HasActiveBatchConditionAlertAsync(int batchId)
        => _alertRepo.HasActiveBatchConditionAlertAsync(batchId);
}
