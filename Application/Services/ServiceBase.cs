using Application.Interfaces;

using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class ServiceBase<T> : IService<T> where T : EntityBase
{
    private readonly IRepository<T> _repository;
    private readonly IUnitOfWork _uow;

    public ServiceBase(IRepository<T> repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public Task<T?> Get(int id) => _repository.GetAsync(id);
    public Task<IEnumerable<T>> GetAll() => _repository.GetAllAsync();

    public async Task<T> Add(T entity)
    {
        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync();   // <-- CRUD SaveChanges
        return entity;
    }

    public async Task Delete(int id)
    {
        await _repository.DeleteAsync(id);
        await _uow.SaveChangesAsync();
    }

    public async Task<T> Update(T entity)
    {
        _repository.Update(entity);
        await _uow.SaveChangesAsync();
        return entity;
    }
}
