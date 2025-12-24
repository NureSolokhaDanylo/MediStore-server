using Domain.Models;
using Application.Results.Base;

namespace Application.Interfaces;

public interface ICrudService<T> : IReadOnlyService<T> where T : EntityBase
{
    Task<Result<T>> Add(string userId, T entity);
    Task<Result<T>> Update(string userId, T entity);
    Task<Result> Delete(string userId, int id);
}
