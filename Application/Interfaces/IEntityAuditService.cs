using Domain.Models;

namespace Application.Interfaces;

public interface IEntityAuditService<T> where T : EntityBase
{
    Task LogCreateAsync(T entity);
    Task LogUpdateAsync(T before, T after);
    Task LogDeleteAsync(T entity);
}
