using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class AuditLogService : ReadOnlyService<AuditLog>, IAuditLogService
{
    private readonly IAuditLogRepository _repo;

    public AuditLogService(IAuditLogRepository repository, IUnitOfWork uow) : base(repository, uow)
    {
        _repo = repository;
    }

    public Task<Result<AuditLog>> GetByIdAsync(int id) => Get(id);

    public async Task<Result<IEnumerable<AuditLog>>> GetByTypeAsync(string entityType, DateTime? from, DateTime? to, int? take)
    {
        var list = await _repo.GetByTypeAsync(entityType, from, to, take);
        return Result<IEnumerable<AuditLog>>.Success(list);
    }
}
