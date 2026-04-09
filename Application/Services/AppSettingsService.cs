using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class AppSettingsService : IAppSettingsService
{
    private readonly IUnitOfWork _uow;
    private readonly IAccessChecker _accessChecker;

    public AppSettingsService(IUnitOfWork uow, IAccessChecker accessChecker)
    {
        _uow = uow;
        _accessChecker = accessChecker;
    }

    // convenience: get single settings (assumes single row)
    public async Task<Result<AppSettings>> GetSingletonAsync()
    {
        var access = _accessChecker.EnsureCurrentUserInAnyRole(["Admin", "Operator"]);
        if (!access.IsSucceed)
            return Result<AppSettings>.Failure(access.Error!);

        var existing = await _uow.AppSettings.GetAsync();
        //åñëè â ðåïîçèîòðèè íåò çàïèñè òî áóäåò âûêèíóòà îøèáêà. Ïîýòîìó ýòî êàê-òî áåññìûñëåííî ïðîâåðÿòü íà null
        return existing is null
            ? Result<AppSettings>.Failure(Errors.NotFound(ErrorCodes.AppSettings.NotFound, "Not found"))
            : Result<AppSettings>.Success(existing);
    }

    public async Task<Result<AppSettings>> Update(AppSettings entity)
    {
        var access = _accessChecker.EnsureCurrentUserInRole("Admin");
        if (!access.IsSucceed)
            return Result<AppSettings>.Failure(access.Error!);

        var existing = await _uow.AppSettings.GetAsync();
        if (existing is null) return Result<AppSettings>.Failure(Errors.NotFound(ErrorCodes.AppSettings.NotFound, "Not found"));

        if (entity.TempAlertDeviation < 0 || entity.TempAlertDeviation > 100)
            return Result<AppSettings>.Failure(Errors.Validation(ErrorCodes.AppSettings.TempAlertDeviationOutOfRange, "TempAlertDeviation out of range", "tempAlertDeviation"));
        if (entity.HumidityAlertDeviation < 0 || entity.HumidityAlertDeviation > 100)
            return Result<AppSettings>.Failure(Errors.Validation(ErrorCodes.AppSettings.HumidityAlertDeviationOutOfRange, "HumidityAlertDeviation out of range", "humidityAlertDeviation"));

        if (entity.ReadingsRetentionDays < 1 || entity.ReadingsRetentionDays > 365 * 5)
            return Result<AppSettings>.Failure(Errors.Validation(ErrorCodes.AppSettings.ReadingsRetentionDaysOutOfRange, "ReadingsRetentionDays out of allowed range (1..1825)", "readingsRetentionDays"));

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
