using Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<List<AuditLog>> GetByTypeAsync(string entityType, DateTime? from = null, DateTime? to = null, int? take = null);
    }
}
