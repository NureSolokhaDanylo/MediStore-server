using Application.Results.Base;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IAuditLogService : IReadOnlyService<AuditLog>
{
    Task<Result<AuditLog>> GetByIdAsync(int id);
    Task<Result<IEnumerable<AuditLog>>> GetByTypeAsync(string entityType, DateTime? from, DateTime? to, int? take);
    Task<Result<(IEnumerable<AuditLog> Items, int TotalCount)>> GetPagedAsync(
        string? q,
        string? entityType,
        string? action,
        string? userId,
        DateTime? from,
        DateTime? to,
        int skip,
        int take);
    Task<Result<(IEnumerable<AuditLog> Items, int TotalCount)>> GetByTypePagedAsync(
        string entityType,
        DateTime? from,
        DateTime? to,
        int skip,
        int take);
}
