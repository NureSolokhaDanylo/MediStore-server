using Application.Interfaces;
using Domain.Models;

namespace Application.Services;

public class NullEntityAuditService<T> : IEntityAuditService<T> where T : EntityBase
{
    public Task LogCreateAsync(T entity) => Task.CompletedTask;

    public Task LogUpdateAsync(T before, T after) => Task.CompletedTask;

    public Task LogDeleteAsync(T entity) => Task.CompletedTask;
}
