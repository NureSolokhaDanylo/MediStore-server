using Application.Interfaces;

using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class AlertService : ServiceBase<Alert>, IAlertService
{
    public AlertService(IAlertRepository repository, IUnitOfWork uow) : base(repository, uow) { }
}
