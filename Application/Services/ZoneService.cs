using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class ZoneService : CrudService<Zone>, IZoneService
{
    public ZoneService(IZoneRepository repository, IUnitOfWork uow) : base(repository, uow) { }

    private Result Validate(Zone z)
    {
        // temperature range
        if (z.TempMin < -50 || z.TempMin > 100)
            return Result.Failure("TempMin must be between -50 and 100");

        if (z.TempMax < -50 || z.TempMax > 100)
            return Result.Failure("TempMax must be between -50 and 100");

        if (z.TempMin > z.TempMax)
            return Result.Failure("TempMin cannot be greater than TempMax");

        // humidity range
        if (z.HumidMin < 0 || z.HumidMin > 100)
            return Result.Failure("HumidMin must be between 0 and 100");

        if (z.HumidMax < 0 || z.HumidMax > 100)
            return Result.Failure("HumidMax must be between 0 and 100");

        if (z.HumidMin > z.HumidMax)
            return Result.Failure("HumidMin cannot be greater than HumidMax");

        return Result.Success();
    }

    public override async Task<Result<Zone>> Add(Zone entity)
    {
        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Zone>.Failure(check.ErrorMessage ?? "Validation failed");

        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync();

        return Result<Zone>.Success(entity);
    }

    public override async Task<Result<Zone>> Update(Zone entity)
    {
        var existing = await _repository.GetAsync(entity.Id);
        if (existing is null)
            return Result<Zone>.Failure("Not found");

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Zone>.Failure(check.ErrorMessage ?? "Validation failed");

        _repository.Update(entity);
        await _uow.SaveChangesAsync();

        return Result<Zone>.Success(entity);
    }
}
