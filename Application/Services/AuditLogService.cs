using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.UOW;

namespace Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IReadOnlyService<AuditLog> _readService;
    private readonly IUnitOfWork _uow;
    private readonly IAccessChecker _accessChecker;

    public AuditLogService(IReadOnlyService<AuditLog> readService, IUnitOfWork uow, IAccessChecker accessChecker)
    {
        _readService = readService;
        _uow = uow;
        _accessChecker = accessChecker;
    }

    public async Task<Result<AuditLog>> Get(int id)
    {
        var access = _accessChecker.EnsureCurrentUserInRole("Admin");
        if (!access.IsSucceed)
            return Result<AuditLog>.Failure(access.Error!);

        return await _readService.Get(id);
    }

    public async Task<Result<IEnumerable<AuditLog>>> GetAll()
    {
        var access = _accessChecker.EnsureCurrentUserInRole("Admin");
        if (!access.IsSucceed)
            return Result<IEnumerable<AuditLog>>.Failure(access.Error!);

        return await _readService.GetAll();
    }

    public Task<Result<AuditLog>> GetByIdAsync(int id) => Get(id);

    public async Task<Result<IEnumerable<AuditLog>>> GetByTypeAsync(string entityType, DateTime? from, DateTime? to, int? take)
    {
        var access = _accessChecker.EnsureCurrentUserInRole("Admin");
        if (!access.IsSucceed)
            return Result<IEnumerable<AuditLog>>.Failure(access.Error!);

        var list = await _uow.AuditLogs.GetByTypeAsync(entityType, from, to, take);
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
        var access = _accessChecker.EnsureCurrentUserInRole("Admin");
        if (!access.IsSucceed)
            return Result<(IEnumerable<AuditLog> Items, int TotalCount)>.Failure(access.Error!);

        if (skip < 0)
        {
            return Result<(IEnumerable<AuditLog> Items, int TotalCount)>.Failure(PagingErrors.InvalidSkip(ErrorCodes.AuditLog.InvalidPaging));
        }

        if (take <= 0)
        {
            return Result<(IEnumerable<AuditLog> Items, int TotalCount)>.Failure(PagingErrors.InvalidTake(ErrorCodes.AuditLog.InvalidPaging, "take must be greater than 0"));
        }

        var result = await _uow.AuditLogs.GetPagedAsync(q, entityType, action, userId, from, to, skip, take);
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
