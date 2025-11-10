using Domain.Models;

namespace Application.Interfaces;

public interface IService<T> where T : EntityBase
{
    Task<T?> Get(int id);
    Task<IEnumerable<T>> GetAll();
    Task<T> Add(T entity);
    Task<T> Update(T entity);
    Task Delete(int id);
}
