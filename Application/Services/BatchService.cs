using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class BatchService : ServiceBase<Batch>, IBatchService
{
    public BatchService(IBatchRepository repository, IUnitOfWork uow) : base(repository, uow) { }

    private Result Validate(Batch b)
    {
        if (b.Quantity <= 0)
            return Result.Failure("Quantity must be greater than 0");

        // DateAdded should not be in the future
        var now = DateTime.UtcNow;
        if (b.DateAdded.ToUniversalTime() > now.AddMinutes(1))
            return Result.Failure("DateAdded cannot be in the future");

        // ExpireDate should be after or equal to DateAdded
        if (b.ExpireDate.ToUniversalTime() < b.DateAdded.ToUniversalTime())
            return Result.Failure("ExpireDate must be after DateAdded");

        return Result.Success();
    }

    public override async Task<Result<Batch>> Add(Batch entity)
    {
        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Batch>.Failure(check.ErrorMessage ?? "Validation failed");

        // ensure referenced entities exist
        var medicine = await _uow.Medicines.GetAsync(entity.MedicineId);
        if (medicine is null)
            return Result<Batch>.Failure("Referenced medicine not found");

        var zone = await _uow.Zones.GetAsync(entity.ZoneId);
        if (zone is null)
            return Result<Batch>.Failure("Referenced zone not found");

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync();

        return Result<Batch>.Success(entity);
    }

    public override async Task<Result<Batch>> Update(Batch entity)
    {
        var existing = await _repository.GetAsync(entity.Id);
        if (existing is null)
            return Result<Batch>.Failure("Not found");

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Batch>.Failure(check.ErrorMessage ?? "Validation failed");

        var medicine = await _uow.Medicines.GetAsync(entity.MedicineId);
        if (medicine is null)
            return Result<Batch>.Failure("Referenced medicine not found");

        var zone = await _uow.Zones.GetAsync(entity.ZoneId);
        if (zone is null)
            return Result<Batch>.Failure("Referenced zone not found");

        _repository.Update(entity);
        await _uow.SaveChangesAsync();

        return Result<Batch>.Success(entity);
    }
}
