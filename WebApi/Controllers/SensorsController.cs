using Application.Interfaces;
using Application.Results.Base;

using Domain.Enums;
using Domain.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/sensors")]
public class SensorsController : ReadController<Sensor, SensorDto, ISensorService>
{
    private readonly ISensorApiKeyService _apiKeyService;
    private readonly ISensorService _sensorService;

    public SensorsController(ISensorService service, ISensorApiKeyService apiKeyService) : base(service) { _apiKeyService = apiKeyService; _sensorService = service; }

    protected override SensorDto ToDto(Sensor entity) => entity.ToDto();
    protected override int GetId(SensorDto dto) => dto.Id;

    [NonAction]
    public override Task<ActionResult<IEnumerable<SensorDto>>> GetAll() => base.GetAll();

    [HttpGet]
    [Authorize(Roles = "Admin,Operator,Observer")]
    public async Task<ActionResult<IEnumerable<SensorDto>>> GetSensors([FromQuery] int? zoneId)
    {
        if (!zoneId.HasValue)
        {
            // If no zoneId provided, return all sensors
            return await GetAll();
        }

        var uid = userId;
        if (string.IsNullOrEmpty(uid)) return UnauthorizedErrorResult<IEnumerable<SensorDto>>();

        var result = await _sensorService.GetByZoneIdAsync(uid, zoneId.Value);
        if (!result.IsSucceed) return ApiErrorResult<IEnumerable<SensorDto>>(result);

        var sensors = result.Value ?? Enumerable.Empty<Sensor>();
        return Ok(sensors.Select(ToDto));
    }

    [HttpGet("paged")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    public async Task<ActionResult<PagedResultDto<SensorDto>>> GetSensorsPaged(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] string? q = null,
        [FromQuery] SensorType? sensorType = null,
        [FromQuery] bool? isOn = null,
        [FromQuery] int? zoneId = null)
    {
        if (skip < 0) return ValidationErrorResult<PagedResultDto<SensorDto>>("skip cannot be negative", ErrorCodes.Sensor.InvalidPaging);
        if (take <= 0) return ValidationErrorResult<PagedResultDto<SensorDto>>("take must be positive", ErrorCodes.Sensor.InvalidPaging);

        var uid = userId;
        if (string.IsNullOrEmpty(uid)) return UnauthorizedErrorResult<PagedResultDto<SensorDto>>();

        var result = await _sensorService.GetPagedAsync(uid, skip, take, q, sensorType, isOn, zoneId);
        if (!result.IsSucceed) return ApiErrorResult<PagedResultDto<SensorDto>>(result);

        var (items, totalCount) = result.Value!;
        return Ok(new PagedResultDto<SensorDto>
        {
            Items = items.Select(ToDto),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        });
    }

    [Authorize(Roles = "Admin,Operator,Observer")]
    public override Task<ActionResult<SensorDto>> Get(int id) => base.Get(id);

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] SensorCreateDto dto)
    {
        var res = await _service.Add(dto.ToEntity());
        if (!res.IsSucceed) return ApiErrorResult(res);
        var created = res.Value!;
        var createdDto = ToDto(created);
        return CreatedAtAction(nameof(Get), new { id = GetId(createdDto) }, createdDto);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _service.Delete(id);
        if (!res.IsSucceed) return ApiErrorResult(res);
        return NoContent();
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAllowedFields([FromBody] SensorUpdateDto dto)
    {
        var res = await _sensorService.UpdateFromAdmin(dto.Id, dto.SerialNumber, dto.IsOn, dto.ZoneId);
        if (!res.IsSucceed) return ApiErrorResult(res);

        return Ok(res.Value!.ToDto());
    }

    [HttpPost("{id:int}/apikey")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateApiKey(int id)
    {
        var res = await _apiKeyService.CreateNewApiKey(id);
        if (!res.IsSucceed) return ApiErrorResult(res);
        return Ok(new { apiKey = res.Value });
    }
}
