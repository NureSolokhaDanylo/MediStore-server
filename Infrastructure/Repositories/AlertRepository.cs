using Domain.Models;

using Infrastructure.Interfaces;

namespace Infrastructure.Repositories
{
    public class AlertRepository : Repository<Alert>, IAlertRepository
    {
        public AlertRepository(AppDbContext context) : base(context) { }
    }
}
