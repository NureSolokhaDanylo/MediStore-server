using Application.Interfaces;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class SensorService : ServiceBase<Sensor>, ISensorService
{
    public SensorService(ISensorRepository repository, IUnitOfWork uow) : base(repository, uow) { }
}
