using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class SensorService : ServiceBase<Sensor>, ISensorService
{
    public SensorService(ISensorRepository repository, IUnitOfWork uow) : base(repository, uow) { }

    //это единственный такой метод, который принимает пол€ вместо сущности. ¬ отличии от остальных тут можно обновить не все пол€
    public async Task<Result<Sensor>> UpdateFromDtoAsync(int id, string? serialNumber, bool? isOn, int? zoneId)
    {
        var existing = await _uow.Sensors.GetAsync(id);
        if (existing is null) return Result<Sensor>.Failure("Not found");

        // only update allowed fields
        if (serialNumber is not null) existing.SerialNumber = serialNumber;
        if (isOn.HasValue) existing.IsOn = isOn.Value;
        if (zoneId.HasValue) existing.ZoneId = zoneId;

        _uow.Sensors.Update(existing);
        await _uow.SaveChangesAsync();

        return Result<Sensor>.Success(existing);
    }
}
