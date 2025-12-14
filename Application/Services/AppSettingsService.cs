using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class AppSettingsService : IAppSettingsService
{
    private readonly IUnitOfWork _uow;
    public AppSettingsService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // convenience: get single settings (assumes single row)
    public async Task<Result<AppSettings>> GetSingletonAsync()
    {
        var list = await _uow.AppSettings.GetAllAsync();
        var first = list.FirstOrDefault();
        return first is null ? Result<AppSettings>.Failure("Not found") : Result<AppSettings>.Success(first);
    }

    public async Task<Result<AppSettings>> Update(AppSettings entity)
    {
        var existing = await _uow.AppSettings.GetAsync(entity.Id);
        if (existing is null) return Result<AppSettings>.Failure("Not found");

        // basic range checks
        if (entity.TempAlertDeviation < 0 || entity.TempAlertDeviation > 100)
            return Result<AppSettings>.Failure("TempAlertDeviation out of range");
        if (entity.HumidityAlertDeviation < 0 || entity.HumidityAlertDeviation > 100)
            return Result<AppSettings>.Failure("HumidityAlertDeviation out of range");

        // update only allowed fields on the existing tracked entity to avoid overwriting other columns
        existing.AlertEnabled = entity.AlertEnabled;
        existing.TempAlertDeviation = entity.TempAlertDeviation;
        existing.HumidityAlertDeviation = entity.HumidityAlertDeviation;

        _uow.AppSettings.Update(existing);
        await _uow.SaveChangesAsync();
        return Result<AppSettings>.Success(existing);
    }
}
