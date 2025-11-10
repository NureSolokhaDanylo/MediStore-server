using Application.Interfaces;

using Domain.Models;

using Infrastructure.Interfaces;

namespace Application.Services;

public class MedicineService : ServiceBase<Medicine>, IMedicineService
{
    public MedicineService(IMedicineRepository repository) : base(repository) { }
}
