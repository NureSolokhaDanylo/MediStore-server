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
            ? Result<T>.Failure(Errors.NotFound(ErrorCodes.Common.NotFound, "Not found", "id", id))
            : Result<T>.Success(entity);
    }

    public async Task<Result<IEnumerable<T>>> GetAll()
    {
        var list = await _uow.GetRepository<T>().GetAllAsync();
        return Result<IEnumerable<T>>.Success(list);
    }
}
