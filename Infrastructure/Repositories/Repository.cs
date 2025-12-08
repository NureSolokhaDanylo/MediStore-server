using Domain.Models;

using Infrastructure.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : EntityBase
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _set;

        public Repository(AppDbContext context)
        {
            _context = context;
            _set = _context.Set<T>();
        }

        public Task<T?> GetAsync(int id)
            => _set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

        public async Task<IEnumerable<T>> GetAllAsync()
            => _set.AsNoTracking().AsEnumerable();

        public Task AddAsync(T entity)
            => _set.AddAsync(entity).AsTask();

        public void Update(T entity)
            => _set.Update(entity);

        public async Task DeleteAsync(int id)
        {
            var entity = await _set.FindAsync(id);
            if (entity != null)
                _set.Remove(entity);
        }
    }
}
