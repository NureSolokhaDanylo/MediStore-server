using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;

using Domain.Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/settings")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class AppSettingsController : MyController
    {
        private readonly IAppSettingsService _settingsService;

        public AppSettingsController(IAppSettingsService appSettingsService)
        {
            _settingsService = appSettingsService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Operator")]
        [ApiErrors(401, 403, 404)]
        [ApiErrorCodes(401, "auth.unauthorized")]
        [ApiErrorCodes(403, "auth.forbidden")]
        [ApiErrorCodes(404, "app_settings.not_found")]
        [ProducesResponseType(typeof(AppSettings), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var res = await _settingsService.GetSingletonAsync();
            if (!res.IsSucceed) return ApiErrorResult(res);
            return Ok(res.Value);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [ApiErrors(400, 401, 403, 404)]
        [ApiErrorCodes(400, "app_settings.temp_alert_deviation_out_of_range", "app_settings.humidity_alert_deviation_out_of_range", "app_settings.readings_retention_days_out_of_range")]
        [ApiErrorCodes(401, "auth.unauthorized")]
        [ApiErrorCodes(403, "auth.forbidden")]
        [ApiErrorCodes(404, "app_settings.not_found")]
        [ProducesResponseType(typeof(AppSettings), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] AppSettingsDto dto)
        {
            var entity = new AppSettings
            {
                AlertEnabled = dto.AlertEnabled,
                TempAlertDeviation = dto.TempAlertDeviation,
                HumidityAlertDeviation = dto.HumidityAlertDeviation,
                CheckDeviationInterval = dto.CheckDeviationInterval,
                ReadingsRetentionDays = dto.ReadingsRetentionDays
            };

            var res = await _settingsService.Update(entity);
            if (!res.IsSucceed) return ApiErrorResult(res);
            return Ok(res.Value);
        }
    }
}
