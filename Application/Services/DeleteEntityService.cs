using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.UOW;

namespace Application.Services;

public class DeleteEntityService<T>(
    IUnitOfWork uow,
    IEntityAuditService<T> auditService) : IDeleteService<T> where T : EntityBase
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IEntityAuditService<T> _auditService = auditService;

    public async Task<Result> Delete(int id)
    {
        var repository = _uow.GetRepository<T>();
        var entity = await repository.GetAsync(id);
        if (entity is null)
            return Result.Failure(Errors.NotFound(GetNotFoundCode(), "Not found", "id", id));

        await repository.DeleteAsync(id);
        await _uow.SaveChangesAsync();

        await _auditService.LogDeleteAsync(entity);

        return Result.Success();
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
