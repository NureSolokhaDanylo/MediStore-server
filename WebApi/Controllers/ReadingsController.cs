using Application.Attributes;
using Application.Interfaces;

using Domain.Models;

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
}
