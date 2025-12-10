using Application.Interfaces;

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

    [Authorize(Roles = "Admin,Operator,Observer")]
    public override Task<ActionResult<IEnumerable<SensorDto>>> GetAll() => base.GetAll();

    [Authorize(Roles = "Admin,Operator,Observer")]
    public override Task<ActionResult<SensorDto>> Get(int id) => base.Get(id);


    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] SensorCreateDto dto)
    {
        var res = await _service.Add(dto.ToEntity());
        if (!res.IsSucceed) return BadRequest(res.ErrorMessage);
        var created = res.Value!;
        var createdDto = ToDto(created);
        return CreatedAtAction(nameof(Get), new { id = GetId(createdDto) }, createdDto);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _service.Delete(id);
        if (!res.IsSucceed) return NotFound(res.ErrorMessage);
        return NoContent();
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAllowedFields([FromBody] SensorUpdateDto dto)
    {
        var res = await _sensorService.UpdateFromAdmin(dto.Id, dto.SerialNumber, dto.IsOn, dto.ZoneId);
        if (!res.IsSucceed) return BadRequest(res.ErrorMessage);

        return Ok(res.Value!.ToDto());
    }

    [HttpPost("{id:int}/apikey")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateApiKey(int id)
    {
        var res = await _apiKeyService.CreateNewApiKey(id);
        if (!res.IsSucceed) return BadRequest(res.ErrorMessage);
        return Ok(new { apiKey = res.Value });
    }
}
