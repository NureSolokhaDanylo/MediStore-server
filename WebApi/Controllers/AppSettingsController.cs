using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;

using Domain.Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/settings")]
    public class AppSettingsController : MyController
    {
        private readonly IAppSettingsService _settingsService;

        public AppSettingsController(IAppSettingsService appSettingsService)
        {
            _settingsService = appSettingsService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> Get()
        {
            var res = await _settingsService.GetSingletonAsync();
            if (!res.IsSucceed) return NotFound(res.ErrorMessage);
            return Ok(res.Value);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] AppSettingsUpdateDto dto)
        {
            var entity = new AppSettings
            {
                Id = dto.Id,
                AlertEnabled = dto.AlertEnabled,
                TempAlertDeviation = dto.TempAlertDeviation,
                HumidityAlertDeviation = dto.HumidityAlertDeviation,
                CheckDeviationInterval = dto.CheckDeviationInterval
            };

            var res = await _settingsService.Update(entity);
            if (!res.IsSucceed) return BadRequest(res.ErrorMessage);
            return Ok(res.Value);
        }
    }
}
