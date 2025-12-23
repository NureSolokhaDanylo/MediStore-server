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
        var existing = await _uow.AppSettings.GetAsync();
        //если в репозиотрии нет записи то будет выкинута ошибка. Поэтому это как-то бессмысленно проверять на null
        return existing is null ? Result<AppSettings>.Failure("Not found") : Result<AppSettings>.Success(existing);
    }

    public async Task<Result<AppSettings>> Update(AppSettings entity)
    {
        var existing = await _uow.AppSettings.GetAsync();
        if (existing is null) return Result<AppSettings>.Failure("Not found");

        if (entity.TempAlertDeviation < 0 || entity.TempAlertDeviation > 100)
            return Result<AppSettings>.Failure("TempAlertDeviation out of range");
        if (entity.HumidityAlertDeviation < 0 || entity.HumidityAlertDeviation > 100)
            return Result<AppSettings>.Failure("HumidityAlertDeviation out of range");

        if (entity.ReadingsRetentionDays < 1 || entity.ReadingsRetentionDays > 365 * 5)
            return Result<AppSettings>.Failure("ReadingsRetentionDays out of allowed range (1..1825)");

        existing.AlertEnabled = entity.AlertEnabled;
        existing.TempAlertDeviation = entity.TempAlertDeviation;
        existing.HumidityAlertDeviation = entity.HumidityAlertDeviation;
        existing.CheckDeviationInterval = entity.CheckDeviationInterval;
        existing.ReadingsRetentionDays = entity.ReadingsRetentionDays;

        await _uow.AppSettings.UpdateAsync(existing);
        await _uow.SaveChangesAsync();
        return Result<AppSettings>.Success(existing);
    }
}
