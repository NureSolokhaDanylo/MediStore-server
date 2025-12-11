using Application.Interfaces;
using Application.Results.Base;

using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class AlertService : CrudService<Alert>, IAlertService
{
    private readonly IAlertRepository _alertRepo;

    public AlertService(IAlertRepository repository, IUnitOfWork uow) : base(repository, uow)
    {
        _alertRepo = repository;
    }

    public async Task<Result> MarkSolvedAsync(int alertId)
    {
        var existing = await _alertRepo.GetAsync(alertId);
        if (existing is null) return Result.Failure("Not found");

        // no IsSolved field now; use SolveTime to indicate solved
        if (existing.SolveTime.HasValue) return Result.Success();

        existing.SolveTime = DateTime.UtcNow;

        _alertRepo.Update(existing);
        await _uow.SaveChangesAsync();

        return Result.Success();
    }
}
