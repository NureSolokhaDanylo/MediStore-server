using Application.Interfaces;
using Application.Results.Base;

using Domain.Enums;
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
            AlertType = AlertType.ZoneConditionAlert,
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

    public Task<Alert?> GetActiveZoneConditionAlertAsync(int zoneId)
        => _alertRepo.GetActiveZoneConditionAlertAsync(zoneId);

    public Task<Alert?> GetActiveBatchConditionAlertAsync(int batchId)
        => _alertRepo.GetActiveBatchConditionAlertAsync(batchId);

    public async Task<Result> AppendToZoneConditionAlertAsync(Alert alert, string message)
    {
        alert.Message = string.Concat(alert.Message, " | ", message);
        _alertRepo.Update(alert);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> ResolveZoneConditionAlertAsync(Alert alert)
    {
        alert.IsActive = false;
        alert.ResolvedAt = DateTime.UtcNow;
        _alertRepo.Update(alert);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> CreateBatchConditionAlertAsync(int batchId, int zoneId, string message)
    {
        var alert = new Alert
        {
            BatchId = batchId,
            ZoneId = zoneId,
            AlertType = AlertType.BatchConditionWarning,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Message = message
        };

        await _alertRepo.AddAsync(alert);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> AppendToBatchConditionAlertAsync(Alert alert, string message)
    {
        alert.Message = string.Concat(alert.Message, " | ", message);
        _alertRepo.Update(alert);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> ResolveBatchConditionAlertAsync(Alert alert)
    {
        alert.IsActive = false;
        alert.ResolvedAt = DateTime.UtcNow;
        _alertRepo.Update(alert);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public Task<bool> HasAlertForBatchAsync(int batchId, AlertType alertType)
        => _alertRepo.HasAlertForBatchAsync(batchId, alertType);

    public async Task<Result> CreateBatchAlertAsync(int batchId, AlertType alertType, string message)
    {
        var alert = new Alert
        {
            BatchId = batchId,
            AlertType = alertType,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Message = message
        };

        await _alertRepo.AddAsync(alert);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }
}
