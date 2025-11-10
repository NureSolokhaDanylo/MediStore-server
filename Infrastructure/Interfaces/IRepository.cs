using Domain.Models;

namespace Infrastructure.Interfaces
{
    public interface IRepository<T> where T : EntityBase
    {
        Task<T?> Get(int id);
        Task<IEnumerable<T>> GetAll();
        Task<T> Add(T t);
        Task<T> Update(T t);
        Task Delete(int id);

    }
}
