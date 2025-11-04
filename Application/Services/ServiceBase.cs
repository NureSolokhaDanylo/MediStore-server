using Application.Interfaces;

using Domain.Models;

using Infrastructure.Interfaces;

namespace Application.Services;

public class ServiceBase<T> : IService<T> where T : EntityBase
{
    private readonly IRepository<T> _repository;
    public ServiceBase(IRepository<T> repository)
    {
        _repository = repository;
    }

    public Task<T?> Get(int id) => _repository.Get(id);
    public Task<IEnumerable<T>> GetAll() => _repository.GetAll();
    public Task<T> Add(T entity) => _repository.Add(entity);
    public Task<T> Update(T entity) => _repository.Update(entity);
    public Task Delete(int id) => _repository.Delete(id);
}
