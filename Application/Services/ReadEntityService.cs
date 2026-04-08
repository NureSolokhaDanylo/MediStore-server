using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;

namespace Application.Services;

public class ReadEntityService<T>(IRepository<T> repository) : IReadOnlyService<T> where T : EntityBase
{
    private readonly IRepository<T> _repository = repository;

    public async Task<Result<T>> Get(int id)
    {
        var entity = await _repository.GetAsync(id);
        return entity is null
            ? Result<T>.Failure(Errors.NotFound(ErrorCodes.Common.NotFound, "Not found", "id", id))
            : Result<T>.Success(entity);
    }

    public async Task<Result<IEnumerable<T>>> GetAll()
    {
        var list = await _repository.GetAllAsync();
        return Result<IEnumerable<T>>.Success(list);
    }
}
