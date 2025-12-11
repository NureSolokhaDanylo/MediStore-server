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

        if (existing.IsSolved) return Result.Success();

        existing.IsSolved = true;
        existing.SolveTime = DateTime.UtcNow;

        _alertRepo.Update(existing);
        await _uow.SaveChangesAsync();

        return Result.Success();
    }
}
