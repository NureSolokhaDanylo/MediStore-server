using Application.Interfaces;

using Domain.Models;

using Infrastructure.Interfaces;

namespace Application.Services;

public class AlertService : ServiceBase<Alert>, IAlertService
{
    public AlertService(IAlertRepository repository) : base(repository) { }
}
