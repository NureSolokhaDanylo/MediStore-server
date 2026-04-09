using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.UOW;

namespace Application.Services;

public class UpdateEntityService<T>(
    IUnitOfWork uow,
    IEntityAuditService<T> auditService) : IUpdateService<T> where T : EntityBase
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IEntityAuditService<T> _auditService = auditService;

    public async Task<Result<T>> Update(T entity)
    {
        var repository = _uow.GetRepository<T>();
        var existing = await repository.GetAsync(entity.Id);
        if (existing is null)
            return Result<T>.Failure(Errors.NotFound(GetNotFoundCode(), "Not found", "id", entity.Id));

        repository.Update(entity);
        await _uow.SaveChangesAsync();

        await _auditService.LogUpdateAsync(existing, entity);

        return Result<T>.Success(entity);
    }

    private static ErrorCode GetNotFoundCode() => typeof(T) switch
    {
        var type when type == typeof(Zone) => ErrorCodes.Zone.NotFound,
        var type when type == typeof(Medicine) => ErrorCodes.Medicine.NotFound,
        var type when type == typeof(Batch) => ErrorCodes.Batch.NotFound,
        var type when type == typeof(Sensor) => ErrorCodes.Sensor.NotFound,
        var type when type == typeof(Alert) => ErrorCodes.Common.NotFound,
        var type when type == typeof(Reading) => ErrorCodes.Common.NotFound,
        var type when type == typeof(AuditLog) => ErrorCodes.Common.NotFound,
        _ => ErrorCodes.Common.NotFound
    };
}
