using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.UOW;

namespace Application.Services;

public class ReadEntityService<T>(IUnitOfWork uow) : IReadOnlyService<T> where T : EntityBase
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<T>> Get(int id)
    {
        var entity = await _uow.GetRepository<T>().GetAsync(id);
        return entity is null
            ? Result<T>.Failure(Errors.NotFound(GetNotFoundCode(), "Not found", "id", id))
            : Result<T>.Success(entity);
    }

    public async Task<Result<IEnumerable<T>>> GetAll()
    {
        var list = await _uow.GetRepository<T>().GetAllAsync();
        return Result<IEnumerable<T>>.Success(list);
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
