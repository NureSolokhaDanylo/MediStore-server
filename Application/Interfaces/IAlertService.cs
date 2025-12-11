using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IAlertService : ICrudService<Alert>
{
    Task<Result> MarkSolvedAsync(int alertId);
}
