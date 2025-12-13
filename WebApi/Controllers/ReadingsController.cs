using Application.Attributes;
using Application.Interfaces;

using Domain.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/readings")]
public class ReadingsController : ReadController<Reading, ReadingDto, IReadingService>
{
    private readonly IReadingService _readingService;

    public ReadingsController(IReadingService service) : base(service) { _readingService = service; }

    protected override ReadingDto ToDto(Reading entity) => entity.ToDto();
    protected override int GetId(ReadingDto dto) => dto.Id;

    [Authorize(Roles = "Admin,Operator,Observer")]
    public override Task<ActionResult<IEnumerable<ReadingDto>>> GetAll() => base.GetAll();

    [Authorize(Roles = "Admin,Operator,Observer")]
    public override Task<ActionResult<ReadingDto>> Get(int id) => base.Get(id);


    [HttpPost]
    [RequireSensorApiKey]
    public async Task<IActionResult> CreateForSensor([FromBody] ReadingCreateDto dto)
    {
        var sid = sensorId;
        if (!sid.HasValue) return Unauthorized();

        var reading = dto.ToEntity();

        var res = await _readingService.CreateForSensorAsync(sid.Value, reading);
        if (!res.IsSucceed) return NoContent(); // if sensor off or similar, do nothing

        var created = res.Value!;
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created.ToDto());
    }

    [HttpGet("sensor/{sensorId}")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    public async Task<IActionResult> GetForSensor([FromRoute] int sensorId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var res = await _readingService.GetReadingsForSensorAsync(sensorId, from.ToUniversalTime(), to.ToUniversalTime());
        if (!res.IsSucceed) return BadRequest(res.ErrorMessage);

        var list = res.Value!.Select(r => r.ToDto());
        return Ok(list);
    }
}
