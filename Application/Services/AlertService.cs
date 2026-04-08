using Application.Interfaces;
using Application.Results.Base;

using Domain.Enums;
using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class AlertService : IAlertService
{
    private readonly IReadOnlyService<Alert> _readService;
    private readonly IAlertRepository _alertRepo;
    private readonly IUnitOfWork _uow;

    public AlertService(IReadOnlyService<Alert> readService, IAlertRepository repository, IUnitOfWork uow)
    {
        _readService = readService;
        _alertRepo = repository;
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

    public async Task<Result<(IEnumerable<Alert> Items, int TotalCount)>> GetFilteredAlertsAsync(
        int skip, 
        int take, 
        bool? isActive = null, 
        int? zoneId = null, 
        int? batchId = null)
    {
        try
        {
            var result = await _alertRepo.GetFilteredAlertsAsync(skip, take, isActive, zoneId, batchId);
            return Result<(IEnumerable<Alert>, int)>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<(IEnumerable<Alert>, int)>.Failure(Errors.Unexpected(ErrorCodes.Alert.RetrievalFailed, $"Error retrieving alerts: {ex.Message}"));
        }
    }
}
