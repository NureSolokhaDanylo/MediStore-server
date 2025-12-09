using Domain.Models;
using Application.Results.Base;

namespace Application.Interfaces;

public interface IService<T> where T : EntityBase
{
    Task<Result<T>> Get(int id);
    Task<Result<IEnumerable<T>>> GetAll();
    Task<Result<T>> Add(T entity);
    Task<Result<T>> Update(T entity);
    Task<Result> Delete(int id);
}
