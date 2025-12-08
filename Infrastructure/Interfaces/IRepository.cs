using Domain.Models;

namespace Infrastructure.Interfaces
{
    public interface IRepository<T> where T : EntityBase
    {
        Task<T?> GetAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        Task DeleteAsync(int id);
    }
}
