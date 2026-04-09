using Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/zones")]
[Consumes("application/json")]
[Produces("application/json")]
public class ZonesController : MyController
{
    private readonly IZoneService _service;
    private readonly ISensorService _sensorService;

    public ZonesController(IZoneService service, ISensorService sensorService)
    {
        _service = service;
        _sensorService = sensorService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ApiErrors(401, 403)]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ProducesResponseType(typeof(IEnumerable<ZoneDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ZoneDto>>> GetAll()
    {
        var res = await _service.GetAll();
        if (!res.IsSucceed) return ApiErrorResult<IEnumerable<ZoneDto>>(res);

        var list = res.Value ?? Enumerable.Empty<Domain.Models.Zone>();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ApiErrors(401, 403, 404)]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ApiErrorCodes(404, "zone.not_found")]
    [ProducesResponseType(typeof(ZoneDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ZoneDto>> Get(int id)
    {
        var res = await _service.Get(id);
        if (!res.IsSucceed) return ApiErrorResult<ZoneDto>(res);

        var entity = res.Value;
        if (entity is null) return NotFound();

        return Ok(ToDto(entity));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ApiErrors(400, 401, 403)]
    [ApiErrorCodes(400, "zone.temp_min_out_of_range", "zone.temp_max_out_of_range", "zone.temp_range_invalid", "zone.humid_min_out_of_range", "zone.humid_max_out_of_range", "zone.humid_range_invalid")]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ProducesResponseType(typeof(ZoneDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] ZoneCreateDto dto)
    {
        var res = await _service.Add(dto.ToEntity());
        if (!res.IsSucceed) return ApiErrorResult(res);

        var createdDto = ToDto(res.Value!);
        return CreatedAtAction(nameof(Get), new { id = createdDto.Id }, createdDto);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    [ApiErrors(400, 401, 403, 404)]
    [ApiErrorCodes(400, "zone.temp_min_out_of_range", "zone.temp_max_out_of_range", "zone.temp_range_invalid", "zone.humid_min_out_of_range", "zone.humid_max_out_of_range", "zone.humid_range_invalid")]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ApiErrorCodes(404, "zone.not_found")]
    [ProducesResponseType(typeof(ZoneDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ZoneDto>> Update([FromBody] ZoneDto dto)
    {
        var res = await _service.Update(dto.ToEntity());
        if (!res.IsSucceed) return ApiErrorResult<ZoneDto>(res);

        return Ok(ToDto(res.Value!));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ApiErrors(400, 401, 403, 404)]
    [ApiErrorCodes(400, "common.validation_error", "zone.temp_min_out_of_range", "zone.temp_max_out_of_range", "zone.temp_range_invalid", "zone.humid_min_out_of_range", "zone.humid_max_out_of_range", "zone.humid_range_invalid")]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ApiErrorCodes(404, "zone.not_found")]
    [ProducesResponseType(typeof(ZoneDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ZoneDto>> Update(int id, [FromBody] ZoneDto dto)
    {
        if (dto.Id != 0 && dto.Id != id)
            return ValidationErrorResult<ZoneDto>("Route id and payload id must match.");

        dto.Id = id;
        return await Update(dto);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ApiErrors(401, 403, 404)]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ApiErrorCodes(404, "zone.not_found")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _service.Delete(id);
        if (!res.IsSucceed) return ApiErrorResult(res);

        return NoContent();
    }

    [HttpGet("search")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ApiErrors(400, 401, 403)]
    [ApiErrorCodes(400, "zone.invalid_search_paging")]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ProducesResponseType(typeof(PagedSearchResultDto<ZoneSearchResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedSearchResultDto<ZoneSearchResultDto>>> Search(
        [FromQuery] string q,
        [FromQuery] int? skip = null,
        [FromQuery] int? take = null,
        [FromQuery] int? offset = null,
        [FromQuery] int? limit = null)
    {
        var effectiveOffset = skip ?? offset ?? 0;
        var effectiveLimit = take ?? limit ?? 10;

        var result = await _service.Search(q, effectiveLimit, effectiveOffset);
        if (!result.IsSucceed) return ApiErrorResult<PagedSearchResultDto<ZoneSearchResultDto>>(result);

        var (items, totalCount) = result.Value!;
        return Ok(new PagedSearchResultDto<ZoneSearchResultDto>
        {
            Items = items.ToSearchResultDto().ToList(),
            TotalCount = totalCount,
            Limit = effectiveLimit,
            Offset = effectiveOffset
        });
    }

    [HttpGet("{id:int}/sensors")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ApiErrors(401, 403, 404, 500)]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ApiErrorCodes(404, "zone.not_found")]
    [ApiErrorCodes(500, "sensor.retrieval_failed")]
    [ProducesResponseType(typeof(IEnumerable<SensorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SensorDto>>> GetSensors(int id)
    {
        // First verify zone exists
        var zoneResult = await _service.Get(id);
        if (!zoneResult.IsSucceed) return ApiErrorResult<IEnumerable<SensorDto>>(zoneResult);

        // Get sensors for this zone
        var sensorsResult = await _sensorService.GetByZoneIdAsync(id);
        if (!sensorsResult.IsSucceed) return ApiErrorResult<IEnumerable<SensorDto>>(sensorsResult);

        var sensors = sensorsResult.Value ?? Enumerable.Empty<Domain.Models.Sensor>();
        return Ok(sensors.Select(s => s.ToDto()));
    }

    private static ZoneDto ToDto(Domain.Models.Zone entity) => entity.ToDto();
}
