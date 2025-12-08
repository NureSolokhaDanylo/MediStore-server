using Application.Interfaces;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class ZoneService : ServiceBase<Zone>, IZoneService
{
    public ZoneService(IZoneRepository repository, IUnitOfWork uow) : base(repository, uow) { }
}
