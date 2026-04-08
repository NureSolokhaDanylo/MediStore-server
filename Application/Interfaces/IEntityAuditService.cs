using Domain.Models;

namespace Application.Interfaces;

public interface IEntityAuditService<T> where T : EntityBase
{
    Task LogCreateAsync(string userId, T entity);
    Task LogUpdateAsync(string userId, T before, T after);
    Task LogDeleteAsync(string userId, T entity);
}
