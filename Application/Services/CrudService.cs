using Application.Interfaces;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;
using Application.Results.Base;

namespace Application.Services;

// Full CRUD service: inherits read operations and adds create/update/delete
public abstract class CrudService<T> : ReadOnlyService<T>, ICrudService<T> where T : EntityBase
{
    public CrudService(IRepository<T> repository, IUnitOfWork uow) : base(repository, uow) { }

    protected abstract Task LogAsync(string userId, string action, T? before, T? after);

    public virtual async Task<Result<T>> Add(string userId, T entity)
    {
        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync();

        await LogAsync(userId, "Create", null, entity);

        return Result<T>.Success(entity);
    }

    public virtual async Task<Result> Delete(string userId, int id)
    {
        var entity = await _repository.GetAsync(id);
        if (entity is null)
            return Result.Failure("Not found");

        await _repository.DeleteAsync(id);
        await _uow.SaveChangesAsync();

        await LogAsync(userId, "Delete", entity, null);

        return Result.Success();
    }

    public virtual async Task<Result<T>> Update(string userId, T entity)
    {
        var existing = await _repository.GetAsync(entity.Id);
        if (existing is null)
            return Result<T>.Failure("Not found");

        _repository.Update(entity);
        await _uow.SaveChangesAsync();

        await LogAsync(userId, "Update", existing, entity);

        return Result<T>.Success(entity);
    }
}
