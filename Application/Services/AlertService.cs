using Application.Interfaces;
using Application.Results.Base;

using Domain.Enums;
using Domain.Models;

using Infrastructure.UOW;

namespace Application.Services;

public class AlertService : IAlertService
{
    private readonly IReadOnlyService<Alert> _readService;
    private readonly IUnitOfWork _uow;

    public AlertService(IReadOnlyService<Alert> readService, IUnitOfWork uow)
    {
        _readService = readService;
        _uow = uow;
    }

    public Task<Result<Alert>> Get(int id) => _readService.Get(id);

    public Task<Result<IEnumerable<Alert>>> GetAll() => _readService.GetAll();

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

        await _uow.Alerts.AddAsync(alert);
        await _uow.SaveChangesAsync();

        return Result.Success();
    }

    public Task<bool> HasActiveZoneConditionAlertAsync(int zoneId)
        => _uow.Alerts.HasActiveZoneConditionAlertAsync(zoneId);

    public Task<bool> HasActiveBatchConditionAlertAsync(int batchId)
        => _uow.Alerts.HasActiveBatchConditionAlertAsync(batchId);

    public Task<Alert?> GetActiveZoneConditionAlertAsync(int zoneId)
        => _uow.Alerts.GetActiveZoneConditionAlertAsync(zoneId);

    public Task<Alert?> GetActiveBatchConditionAlertAsync(int batchId)
        => _uow.Alerts.GetActiveBatchConditionAlertAsync(batchId);

    public async Task<Result> AppendToZoneConditionAlertAsync(Alert alert, string message)
    {
        alert.Message = string.Concat(alert.Message, " | ", message);
        _uow.Alerts.Update(alert);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> ResolveZoneConditionAlertAsync(Alert alert)
    {
        alert.IsActive = false;
        alert.ResolvedAt = DateTime.UtcNow;
        _uow.Alerts.Update(alert);
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

        await _uow.Alerts.AddAsync(alert);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> AppendToBatchConditionAlertAsync(Alert alert, string message)
    {
        alert.Message = string.Concat(alert.Message, " | ", message);
        _uow.Alerts.Update(alert);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> ResolveBatchConditionAlertAsync(Alert alert)
    {
        alert.IsActive = false;
        alert.ResolvedAt = DateTime.UtcNow;
        _uow.Alerts.Update(alert);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public Task<bool> HasAlertForBatchAsync(int batchId, AlertType alertType)
        => _uow.Alerts.HasAlertForBatchAsync(batchId, alertType);

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

        await _uow.Alerts.AddAsync(alert);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<(IEnumerable<Alert> Items, int TotalCount)>> GetFilteredAlertsAsync(
        int skip, 
        int take, 
        bool? isActive = null, 
        int? zoneId = null, 
        int? batchId = null)
    {
        try
        {
            var result = await _uow.Alerts.GetFilteredAlertsAsync(skip, take, isActive, zoneId, batchId);
            return Result<(IEnumerable<Alert>, int)>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<(IEnumerable<Alert>, int)>.Failure(Errors.Unexpected(ErrorCodes.Alert.RetrievalFailed, $"Error retrieving alerts: {ex.Message}"));
        }
    }
}
