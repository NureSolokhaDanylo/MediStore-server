using Domain.Models;
using Infrastructure.Interfaces;

namespace Infrastructure.Repositories
{
    internal class MedicineRepository : Repository<Medicine>, IMedicineRepository
    {
        public MedicineRepository(AppDbContext context) : base(context) { }
    }
}
