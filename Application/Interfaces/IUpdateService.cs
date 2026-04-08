using Application.Results.Base;
using Domain.Models;

namespace Application.Interfaces;

public interface IUpdateService<T> where T : EntityBase
{
    Task<Result<T>> Update(T entity);
}
