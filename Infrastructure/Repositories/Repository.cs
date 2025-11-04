using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<T> Add(T t)
        {
            await _set.AddAsync(t);
            await _context.SaveChangesAsync();
            return t;
        }

        public async Task Delete(int id)
        {
            var entity = await _set.FindAsync(id);
            if (entity is null) return;
            _set.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<T?> Get(int id)
        {
            return await _set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _set.AsNoTracking().ToListAsync();
        }

        public async Task<T> Update(T t)
        {
            _context.Entry(t).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return t;
        }
    }
}
