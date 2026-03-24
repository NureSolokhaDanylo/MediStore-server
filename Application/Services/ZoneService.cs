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
            return Result.Failure("TempMin must be between -50 and 100");

        if (z.TempMax < -50 || z.TempMax > 100)
            return Result.Failure("TempMax must be between -50 and 100");

        if (z.TempMin > z.TempMax)
            return Result.Failure("TempMin cannot be greater than TempMax");

        if (z.HumidMin < 0 || z.HumidMin > 100)
            return Result.Failure("HumidMin must be between 0 and 100");

        if (z.HumidMax < 0 || z.HumidMax > 100)
            return Result.Failure("HumidMax must be between 0 and 100");

        if (z.HumidMin > z.HumidMax)
            return Result.Failure("HumidMin cannot be greater than HumidMax");

        return Result.Success();
    }

    public override async Task<Result<Zone>> Add(string userId, Zone entity)
    {
        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Zone>.Failure(check.ErrorMessage ?? "Validation failed");

        return await base.Add(userId, entity);
    }

    public override async Task<Result<Zone>> Update(string userId, Zone entity)
    {
        var existing = await _repository.GetAsync(entity.Id);
        if (existing is null)
            return Result<Zone>.Failure("Not found");

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Zone>.Failure(check.ErrorMessage ?? "Validation failed");

        return await base.Update(userId, entity);
    }

    public async Task<Result<(IEnumerable<Zone> items, int totalCount)>> Search(string userId, string query, int limit, int offset)
    {
        if (limit <= 0)
            return Result<(IEnumerable<Zone>, int)>.Failure("Limit must be greater than 0");

        if (offset < 0)
            return Result<(IEnumerable<Zone>, int)>.Failure("Offset cannot be negative");

        var (items, totalCount) = await _zoneRepository.SearchAsync(query?.Trim() ?? "", limit, offset);
        return Result<(IEnumerable<Zone>, int)>.Success((items, totalCount));
    }

    protected override async Task LogAsync(string userId, string action, Zone? before, Zone? after)
    {
        var id = after?.Id ?? before?.Id ?? 0;
        var log = new AuditLog
        {
            OccurredAt = DateTime.UtcNow,
            EntityType = "Zone",
            EntityId = id,
            Action = action,
            UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
            Summary = $"Zone {action} (Id={id})",
            OldValues = before is null ? null : JsonSerializer.Serialize(before),
            NewValues = after is null ? null : JsonSerializer.Serialize(after)
        };

        await _uow.AuditLogs.AddAsync(log);
        await _uow.SaveChangesAsync();
    }
}
