using Application.Interfaces;
using Domain.Models;
using Infrastructure.Interfaces;

namespace Application.Services;

public class ZoneService : ServiceBase<Zone>, IZoneService
{
 public ZoneService(IZoneRepository repository) : base(repository) { }
}
