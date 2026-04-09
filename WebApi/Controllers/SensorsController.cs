using Application.Interfaces;
using Application.Results.Base;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/sensors")]
[Consumes("application/json")]
[Produces("application/json")]
public class SensorsController : MyController
{
    private readonly ISensorApiKeyService _apiKeyService;
    private readonly ISensorService _sensorService;

    public SensorsController(ISensorService service, ISensorApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
        _sensorService = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ApiErrors(401, 403, 500)]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ApiErrorCodes(500, "sensor.retrieval_failed")]
    [ProducesResponseType(typeof(IEnumerable<SensorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SensorDto>>> GetSensors([FromQuery] int? zoneId)
    {
        if (!zoneId.HasValue)
        {
            var allResult = await _sensorService.GetAll();
            if (!allResult.IsSucceed) return ApiErrorResult<IEnumerable<SensorDto>>(allResult);

            var allSensors = allResult.Value ?? Enumerable.Empty<Domain.Models.Sensor>();
            return Ok(allSensors.Select(ToDto));
        }

        var result = await _sensorService.GetByZoneIdAsync(zoneId.Value);
        if (!result.IsSucceed) return ApiErrorResult<IEnumerable<SensorDto>>(result);

        var sensors = result.Value ?? Enumerable.Empty<Domain.Models.Sensor>();
        return Ok(sensors.Select(ToDto));
    }

    [HttpGet("paged")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ApiErrors(400, 401, 403, 500)]
    [ApiErrorCodes(400, "sensor.invalid_paging")]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ApiErrorCodes(500, "sensor.retrieval_failed")]
    [ProducesResponseType(typeof(PagedResultDto<SensorDto>), StatusCodes.Status200OK)]
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

        var result = await _sensorService.GetPagedAsync(skip, take, q, sensorType, isOn, zoneId);
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

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ApiErrors(401, 403, 404)]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ApiErrorCodes(404, "sensor.not_found")]
    [ProducesResponseType(typeof(SensorDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SensorDto>> Get(int id)
    {
        var res = await _sensorService.Get(id);
        if (!res.IsSucceed) return ApiErrorResult<SensorDto>(res);

        var entity = res.Value;
        if (entity is null) return NotFound();

        return Ok(ToDto(entity));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ApiErrors(401, 403, 404)]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ApiErrorCodes(404, "sensor.zone_not_found")]
    [ProducesResponseType(typeof(SensorDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] SensorCreateDto dto)
    {
        var res = await _sensorService.Add(dto.ToEntity());
        if (!res.IsSucceed) return ApiErrorResult(res);
        var created = res.Value!;
        var createdDto = ToDto(created);
        return CreatedAtAction(nameof(Get), new { id = createdDto.Id }, createdDto);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    [ApiErrors(401, 403, 404)]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ApiErrorCodes(404, "sensor.not_found")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        var res = await _sensorService.Delete(id);
        if (!res.IsSucceed) return ApiErrorResult(res);
        return NoContent();
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    [ApiErrors(401, 403, 404)]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ApiErrorCodes(404, "sensor.not_found")]
    [ProducesResponseType(typeof(SensorDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAllowedFields([FromBody] SensorUpdateDto dto)
    {
        var res = await _sensorService.UpdateFromAdmin(dto.Id, dto.SerialNumber, dto.IsOn, dto.ZoneId);
        if (!res.IsSucceed) return ApiErrorResult(res);

        return Ok(res.Value!.ToDto());
    }

    // NOTE: API response contract changed from an anonymous object to SensorApiKeyResponseDto.
    [HttpPost("{id:int}/apikey")]
    [Authorize(Roles = "Admin")]
    [ApiErrors(401, 403, 404)]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ApiErrorCodes(404, "sensor_api_key.sensor_not_found")]
    [ProducesResponseType(typeof(SensorApiKeyResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateApiKey(int id)
    {
        var res = await _apiKeyService.CreateNewApiKey(id);
        if (!res.IsSucceed) return ApiErrorResult(res);
        return Ok(new SensorApiKeyResponseDto
        {
            ApiKey = res.Value!
        });
    }

    private static SensorDto ToDto(Domain.Models.Sensor entity) => entity.ToDto();
}
