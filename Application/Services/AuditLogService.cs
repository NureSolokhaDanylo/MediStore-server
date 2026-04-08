using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;

namespace Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IReadOnlyService<AuditLog> _readService;
    private readonly IAuditLogRepository _repo;

    public AuditLogService(IReadOnlyService<AuditLog> readService, IAuditLogRepository repository)
    {
        _readService = readService;
        _repo = repository;
    }

    public Task<Result<AuditLog>> Get(int id) => _readService.Get(id);

    public Task<Result<IEnumerable<AuditLog>>> GetAll() => _readService.GetAll();

    public Task<Result<AuditLog>> GetByIdAsync(int id) => Get(id);

    public async Task<Result<IEnumerable<AuditLog>>> GetByTypeAsync(string entityType, DateTime? from, DateTime? to, int? take)
    {
        var list = await _repo.GetByTypeAsync(entityType, from, to, take);
        return Result<IEnumerable<AuditLog>>.Success(list);
    }

    public async Task<Result<(IEnumerable<AuditLog> Items, int TotalCount)>> GetPagedAsync(
        string? q,
        string? entityType,
        string? action,
        string? userId,
        DateTime? from,
        DateTime? to,
        int skip,
        int take)
    {
        if (skip < 0)
        {
            return Result<(IEnumerable<AuditLog> Items, int TotalCount)>.Failure(PagingErrors.InvalidSkip(ErrorCodes.AuditLog.InvalidPaging));
        }

        if (take <= 0)
        {
            return Result<(IEnumerable<AuditLog> Items, int TotalCount)>.Failure(PagingErrors.InvalidTake(ErrorCodes.AuditLog.InvalidPaging, "take must be greater than 0"));
        }

        var result = await _repo.GetPagedAsync(q, entityType, action, userId, from, to, skip, take);
        return Result<(IEnumerable<AuditLog> Items, int TotalCount)>.Success(result);
    }

    public async Task<Result<(IEnumerable<AuditLog> Items, int TotalCount)>> GetByTypePagedAsync(
        string entityType,
        DateTime? from,
        DateTime? to,
        int skip,
        int take)
    {
        return await GetPagedAsync(null, entityType, null, null, from, to, skip, take);
    }
}
