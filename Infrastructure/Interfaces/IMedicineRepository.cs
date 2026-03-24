using Domain.Models;

namespace Infrastructure.Interfaces
{
    public interface IMedicineRepository : IRepository<Medicine>
    {
        Task<(List<Medicine> items, int totalCount)> SearchAsync(string query, int limit, int offset);
    }
}
