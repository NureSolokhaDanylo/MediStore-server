using Application.Results.Base;
using Domain.Models;

namespace Application.Interfaces;

public interface ICreateService<T> where T : EntityBase
{
    Task<Result<T>> Add(T entity);
}
