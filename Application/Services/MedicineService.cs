using Application.Interfaces;

using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class MedicineService : ServiceBase<Medicine>, IMedicineService
{
    public MedicineService(IMedicineRepository repository, IUnitOfWork uow) : base(repository, uow) { }
}
