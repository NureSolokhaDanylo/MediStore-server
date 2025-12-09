using Application.Interfaces;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;
using Application.Results.Base;

namespace Application.Services;

public class ServiceBase<T> : IService<T> where T : EntityBase
{
    protected readonly IRepository<T> _repository;
    protected readonly IUnitOfWork _uow;

    public ServiceBase(IRepository<T> repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task<Result<T>> Get(int id)
    {
        var entity = await _repository.GetAsync(id);
        return entity is null ? Result<T>.Failure("Not found") : Result<T>.Success(entity);
    }

    public async Task<Result<IEnumerable<T>>> GetAll()
    {
        var list = await _repository.GetAllAsync();
        return Result<IEnumerable<T>>.Success(list);
    }

    public virtual async Task<Result<T>> Add(T entity)
    {
        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync();   // <-- CRUD SaveChanges
        return Result<T>.Success(entity);
    }

    public async Task<Result> Delete(int id)
    {
        var entity = await _repository.GetAsync(id);
        if (entity is null)
            return Result.Failure("Not found");

        await _repository.DeleteAsync(id);
        await _uow.SaveChangesAsync();
        return Result.Success();
    }

    public virtual async Task<Result<T>> Update(T entity)
    {
        var existing = await _repository.GetAsync(entity.Id);
        if (existing is null)
            return Result<T>.Failure("Not found");

        _repository.Update(entity);
        await _uow.SaveChangesAsync();
        return Result<T>.Success(entity);
    }
}
