using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;
using System.Text.Json;

namespace Application.Services;

public class ZoneService : CrudService<Zone>, IZoneService
{
    private readonly IZoneRepository _zoneRepository;

    public ZoneService(IZoneRepository repository, IUnitOfWork uow) : base(repository, uow) 
    { 
        _zoneRepository = repository;
    }

    private Result Validate(Zone z)
    {
        if (z.TempMin < -50 || z.TempMin > 100)
            return Result.Failure(new ErrorInfo
            {
                Code = "zone.validation_failed",
                Message = "TempMin must be between -50 and 100",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "tempMin" }
            });

        if (z.TempMax < -50 || z.TempMax > 100)
            return Result.Failure(new ErrorInfo
            {
                Code = "zone.validation_failed",
                Message = "TempMax must be between -50 and 100",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "tempMax" }
            });

        if (z.TempMin > z.TempMax)
            return Result.Failure(new ErrorInfo
            {
                Code = "zone.validation_failed",
                Message = "TempMin cannot be greater than TempMax",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "tempMin" }
            });

        if (z.HumidMin < 0 || z.HumidMin > 100)
            return Result.Failure(new ErrorInfo
            {
                Code = "zone.validation_failed",
                Message = "HumidMin must be between 0 and 100",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "humidMin" }
            });

        if (z.HumidMax < 0 || z.HumidMax > 100)
            return Result.Failure(new ErrorInfo
            {
                Code = "zone.validation_failed",
                Message = "HumidMax must be between 0 and 100",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "humidMax" }
            });

        if (z.HumidMin > z.HumidMax)
            return Result.Failure(new ErrorInfo
            {
                Code = "zone.validation_failed",
                Message = "HumidMin cannot be greater than HumidMax",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "humidMin" }
            });

        return Result.Success();
    }

    public override async Task<Result<Zone>> Add(string userId, Zone entity)
    {
        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Zone>.Failure(check.Error!);

        return await base.Add(userId, entity);
    }

    public override async Task<Result<Zone>> Update(string userId, Zone entity)
    {
        var existing = await _repository.GetAsync(entity.Id);
        if (existing is null)
            return Result<Zone>.Failure(new ErrorInfo
            {
                Code = "zone.not_found",
                Message = "Not found",
                Type = ErrorType.NotFound,
                Details = new Dictionary<string, object?> { ["zoneId"] = entity.Id }
            });

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Zone>.Failure(check.Error!);

        return await base.Update(userId, entity);
    }

    public async Task<Result<(IEnumerable<Zone> items, int totalCount)>> Search(string userId, string query, int limit, int offset)
    {
        if (limit <= 0)
            return Result<(IEnumerable<Zone>, int)>.Failure(new ErrorInfo
            {
                Code = "zone.invalid_search_paging",
                Message = "Limit must be greater than 0",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "limit" }
            });

        if (offset < 0)
            return Result<(IEnumerable<Zone>, int)>.Failure(new ErrorInfo
            {
                Code = "zone.invalid_search_paging",
                Message = "Offset cannot be negative",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "offset" }
            });

        var (items, totalCount) = await _zoneRepository.SearchAsync(query?.Trim() ?? "", limit, offset);
        return Result<(IEnumerable<Zone>, int)>.Success((items, totalCount));
    }

    protected override async Task LogAsync(string userId, string action, Zone? before, Zone? after)
    {
        var id = after?.Id ?? before?.Id ?? 0;
        var beforeSnapshot = before is null
            ? null
            : new
            {
                before.Id,
                before.Name,
                before.Description,
                before.TempMax,
                before.TempMin,
                before.HumidMax,
                before.HumidMin
            };

        var afterSnapshot = after is null
            ? null
            : new
            {
                after.Id,
                after.Name,
                after.Description,
                after.TempMax,
                after.TempMin,
                after.HumidMax,
                after.HumidMin
            };

        var log = new AuditLog
        {
            OccurredAt = DateTime.UtcNow,
            EntityType = "Zone",
            EntityId = id,
            Action = action,
            UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
            Summary = $"Zone {action} (Id={id})",
            OldValues = beforeSnapshot is null ? null : JsonSerializer.Serialize(beforeSnapshot),
            NewValues = afterSnapshot is null ? null : JsonSerializer.Serialize(afterSnapshot)
        };

        await _uow.AuditLogs.AddAsync(log);
        await _uow.SaveChangesAsync();
    }
}
