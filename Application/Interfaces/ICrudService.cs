using Domain.Models;
using Application.Results.Base;

namespace Application.Interfaces;

public interface ICrudService<T> : IReadOnlyService<T> where T : EntityBase
{
    Task<Result<T>> Add(T entity);
    Task<Result<T>> Update(T entity);
    Task<Result> Delete(int id);
}
