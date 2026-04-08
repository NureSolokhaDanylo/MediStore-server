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
        //åñëè â ðåïîçèîòðèè íåò çàïèñè òî áóäåò âûêèíóòà îøèáêà. Ïîýòîìó ýòî êàê-òî áåññìûñëåííî ïðîâåðÿòü íà null
        return existing is null
            ? Result<AppSettings>.Failure(new ErrorInfo
            {
                Code = "app_settings.not_found",
                Message = "Not found",
                Type = ErrorType.NotFound
            })
            : Result<AppSettings>.Success(existing);
    }

    public async Task<Result<AppSettings>> Update(AppSettings entity)
    {
        var existing = await _uow.AppSettings.GetAsync();
        if (existing is null) return Result<AppSettings>.Failure(new ErrorInfo
        {
            Code = "app_settings.not_found",
            Message = "Not found",
            Type = ErrorType.NotFound
        });

        if (entity.TempAlertDeviation < 0 || entity.TempAlertDeviation > 100)
            return Result<AppSettings>.Failure(new ErrorInfo
            {
                Code = "app_settings.validation_failed",
                Message = "TempAlertDeviation out of range",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "tempAlertDeviation" }
            });
        if (entity.HumidityAlertDeviation < 0 || entity.HumidityAlertDeviation > 100)
            return Result<AppSettings>.Failure(new ErrorInfo
            {
                Code = "app_settings.validation_failed",
                Message = "HumidityAlertDeviation out of range",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "humidityAlertDeviation" }
            });

        if (entity.ReadingsRetentionDays < 1 || entity.ReadingsRetentionDays > 365 * 5)
            return Result<AppSettings>.Failure(new ErrorInfo
            {
                Code = "app_settings.validation_failed",
                Message = "ReadingsRetentionDays out of allowed range (1..1825)",
                Type = ErrorType.Validation,
                Details = new Dictionary<string, object?> { ["field"] = "readingsRetentionDays" }
            });

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
