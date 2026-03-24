using Application.Interfaces;

using Domain.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/zones")]
public class ZonesController : CrudController<Zone, ZoneDto, ZoneCreateDto, IZoneService>
{
    private readonly ISensorService _sensorService;

    public ZonesController(IZoneService service, ISensorService sensorService) : base(service) 
    {
        _sensorService = sensorService;
    }

    protected override ZoneDto ToDto(Zone entity) => entity.ToDto();
    protected override Zone ToEntity(ZoneDto dto) => dto.ToEntity();
    protected override Zone ToEntity(ZoneCreateDto dto) => dto.ToEntity();
    protected override int GetId(ZoneDto dto) => dto.Id;

    [Authorize(Roles = "Admin,Operator,Observer")]
    public override Task<ActionResult<IEnumerable<ZoneDto>>> GetAll() => base.GetAll();

    [Authorize(Roles = "Admin,Operator,Observer")]
    public override Task<ActionResult<ZoneDto>> Get(int id) => base.Get(id);

    [Authorize(Roles = "Admin")]
    public override Task<IActionResult> Create([FromBody] ZoneCreateDto dto) => base.Create(dto);

    [Authorize(Roles = "Admin")]
    public override Task<ActionResult<ZoneDto>> Update([FromBody] ZoneDto dto) => base.Update(dto);

    [Authorize(Roles = "Admin")]
    public override Task<IActionResult> Delete(int id) => base.Delete(id);

    [HttpGet("search")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    public async Task<ActionResult<PagedSearchResultDto<ZoneSearchResultDto>>> Search(
        [FromQuery] string q,
        [FromQuery] int limit = 10,
        [FromQuery] int offset = 0)
    {
        var uid = userId;
        if (string.IsNullOrEmpty(uid)) return Unauthorized();

        var result = await _service.Search(uid, q, limit, offset);
        if (!result.IsSucceed) return BadRequest(result.ErrorMessage);

        var (items, totalCount) = result.Value!;
        return Ok(new PagedSearchResultDto<ZoneSearchResultDto>
        {
            Items = items.ToSearchResultDto().ToList(),
            TotalCount = totalCount,
            Limit = limit,
            Offset = offset
        });
    }

    [HttpGet("{id:int}/sensors")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    public async Task<ActionResult<IEnumerable<SensorDto>>> GetSensors(int id)
    {
        var uid = userId;
        if (string.IsNullOrEmpty(uid)) return Unauthorized();

        // First verify zone exists
        var zoneResult = await _service.Get(id);
        if (!zoneResult.IsSucceed) return NotFound(zoneResult.ErrorMessage);

        // Get sensors for this zone
        var sensorsResult = await _sensorService.GetByZoneIdAsync(uid, id);
        if (!sensorsResult.IsSucceed) return BadRequest(sensorsResult.ErrorMessage);

        var sensors = sensorsResult.Value ?? Enumerable.Empty<Sensor>();
        return Ok(sensors.Select(s => s.ToDto()));
    }
}
