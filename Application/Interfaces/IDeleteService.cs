using Application.Results.Base;
using Domain.Models;

namespace Application.Interfaces;

public interface IDeleteService<T> where T : EntityBase
{
    Task<Result> Delete(int id);
}
