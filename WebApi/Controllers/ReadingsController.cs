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
    private readonly ISensorService _sensorService;

    public ReadingsController(IReadingService service, ISensorService sensorService) : base(service) { _sensorService = sensorService; }

    protected override ReadingDto ToDto(Reading entity) => entity.ToDto();
    protected override int GetId(ReadingDto dto) => dto.Id;

    [HttpPost]
    [RequireSensorApiKey]
    public async Task<IActionResult> CreateForSensor([FromBody] ReadingCreateDto dto)
    {
        // sensorId comes from MyController (set by middleware)
        var sid = sensorId;
        if (!sid.HasValue) return Unauthorized();

        // check sensor
        var sensorRes = await _sensorService.Get(sid.Value);
        if (!sensorRes.IsSucceed) return BadRequest(sensorRes.ErrorMessage);
        var sensor = sensorRes.Value!;
        if (!sensor.IsOn) return NoContent(); // do nothing if sensor is off

        // prepare reading
        var reading = dto.ToEntity();
        reading.SensorId = sid.Value;

        var addRes = await _service.Add(reading);
        if (!addRes.IsSucceed) return BadRequest(addRes.ErrorMessage);
        var created = addRes.Value!;

        // update sensor last value and last update
        sensor.LastValue = created.Value;
        sensor.LastUpdate = DateTime.UtcNow;
        var updateSensorRes = await _sensorService.Update(sensor);
        if (!updateSensorRes.IsSucceed) return BadRequest(updateSensorRes.ErrorMessage);

        var createdDto = created.ToDto();
        return CreatedAtAction(nameof(Get), new { id = createdDto.Id }, createdDto);
    }
}
