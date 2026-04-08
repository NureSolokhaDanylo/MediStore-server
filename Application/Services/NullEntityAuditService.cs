using Application.Interfaces;
using Domain.Models;

namespace Application.Services;

public class NullEntityAuditService<T> : IEntityAuditService<T> where T : EntityBase
{
    public Task LogCreateAsync(string userId, T entity) => Task.CompletedTask;

    public Task LogUpdateAsync(string userId, T before, T after) => Task.CompletedTask;

    public Task LogDeleteAsync(string userId, T entity) => Task.CompletedTask;
}
