using Application.Attributes;
using Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/readings")]
[Consumes("application/json")]
[Produces("application/json")]
public class ReadingsController : MyController
{
    private readonly IReadingService _readingService;

    public ReadingsController(IReadingService service)
    {
        _readingService = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ApiErrors(401, 403, Codes = new[] { "auth.unauthorized", "auth.forbidden" })]
    [ProducesResponseType(typeof(IEnumerable<ReadingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReadingDto>>> GetAll()
    {
        var res = await _readingService.GetAll();
        if (!res.IsSucceed) return ApiErrorResult<IEnumerable<ReadingDto>>(res);

        var list = res.Value ?? Enumerable.Empty<Domain.Models.Reading>();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ApiErrors(401, 403, 404, Codes = new[] { "auth.unauthorized", "auth.forbidden", "common.not_found" })]
    [ProducesResponseType(typeof(ReadingDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReadingDto>> Get(int id)
    {
        var res = await _readingService.Get(id);
        if (!res.IsSucceed) return ApiErrorResult<ReadingDto>(res);

        var entity = res.Value;
        if (entity is null) return NotFound();

        return Ok(ToDto(entity));
    }

    [HttpPost]
    [RequireSensorApiKey]
    [ApiErrors(401, Codes = new[] { "sensor_api_key.empty_key", "sensor_api_key.invalid_key", "sensor_api_key.not_bound_to_sensor" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreateForSensor([FromBody] ReadingCreateDto dto)
    {
        var reading = dto.ToEntity();

        var res = await _readingService.CreateForSensorAsync(reading);
        if (!res.IsSucceed) return NoContent(); // if sensor off or similar, do nothing

        return NoContent();
    }

    [HttpGet("sensor/{sensorId}")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ApiErrors(400, 401, 403, Codes = new[] { "reading.invalid_time_range", "auth.unauthorized", "auth.forbidden" })]
    [ProducesResponseType(typeof(IEnumerable<ReadingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForSensor([FromRoute] int sensorId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var res = await _readingService.GetReadingsForSensorAsync(sensorId, from.ToUniversalTime(), to.ToUniversalTime());
        if (!res.IsSucceed) return ApiErrorResult(res);

        var list = res.Value!.Select(r => r.ToDto());
        return Ok(list);
    }

    [HttpGet("sensor/{sensorId}/last")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ApiErrors(400, 401, 403, Codes = new[] { "reading.invalid_count", "auth.unauthorized", "auth.forbidden" })]
    [ProducesResponseType(typeof(IEnumerable<ReadingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLastForSensor([FromRoute] int sensorId, [FromQuery] int count)
    {
        var res = await _readingService.GetLatestReadingsForSensorAsync(sensorId, count);
        if (!res.IsSucceed) return ApiErrorResult(res);

        var list = res.Value!.Select(r => r.ToDto());
        return Ok(list);
    }

    [HttpGet("zone/{zoneId}")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ApiErrors(400, 401, 403, Codes = new[] { "reading.invalid_time_range", "auth.unauthorized", "auth.forbidden" })]
    [ProducesResponseType(typeof(IEnumerable<ReadingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForZone([FromRoute] int zoneId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var res = await _readingService.GetReadingsForZoneAsync(zoneId, from.ToUniversalTime(), to.ToUniversalTime());
        if (!res.IsSucceed) return ApiErrorResult(res);

        var list = res.Value!.Select(r => r.ToDto());
        return Ok(list);
    }

    [HttpGet("zone/{zoneId}/last")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ApiErrors(400, 401, 403, Codes = new[] { "reading.invalid_count", "auth.unauthorized", "auth.forbidden" })]
    [ProducesResponseType(typeof(IEnumerable<ReadingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLastForZone([FromRoute] int zoneId, [FromQuery] int count)
    {
        var res = await _readingService.GetLatestReadingsForZoneAsync(zoneId, count);
        if (!res.IsSucceed) return ApiErrorResult(res);

        var list = res.Value!.Select(r => r.ToDto());
        return Ok(list);
    }

    private static ReadingDto ToDto(Domain.Models.Reading entity) => entity.ToDto();
}
