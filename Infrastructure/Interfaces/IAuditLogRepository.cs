using Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<List<AuditLog>> GetByTypeAsync(string entityType, DateTime? from = null, DateTime? to = null, int? take = null);
        Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(
            string? q = null,
            string? entityType = null,
            string? action = null,
            string? userId = null,
            DateTime? from = null,
            DateTime? to = null,
            int skip = 0,
            int take = 50);
        Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetByTypePagedAsync(
            string entityType,
            DateTime? from = null,
            DateTime? to = null,
            int skip = 0,
            int take = 50);
    }
}
