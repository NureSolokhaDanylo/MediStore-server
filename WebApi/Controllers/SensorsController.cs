using Application.Interfaces;

using Domain.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/sensors")]
public class SensorsController : CrudController<Sensor, SensorDto, SensorCreateDto, ISensorService>
{
    private readonly ISensorApiKeyService _apiKeyService;

    public SensorsController(ISensorService service, ISensorApiKeyService apiKeyService) : base(service) { _apiKeyService = apiKeyService; }

    protected override SensorDto ToDto(Sensor entity) => entity.ToDto();
    protected override Sensor ToEntity(SensorDto dto) => dto.ToEntity();
    protected override Sensor ToEntity(SensorCreateDto dto) => dto.ToEntity();
    protected override int GetId(SensorDto dto) => dto.Id;

    [Authorize(Roles = "Admin,Operator,Observer")]
    public override Task<ActionResult<IEnumerable<SensorDto>>> GetAll() => base.GetAll();

    [Authorize(Roles = "Admin,Operator,Observer")]
    public override Task<ActionResult<SensorDto>> Get(int id) => base.Get(id);

    [Authorize(Roles = "Admin")]
    public override Task<IActionResult> Create([FromBody] SensorCreateDto dto) => base.Create(dto);

    [Authorize(Roles = "Admin")]
    //нельзя менять LastValue и LastUpdate
    public override Task<ActionResult<SensorDto>> Update([FromBody] SensorDto dto) => base.Update(dto);

    [Authorize(Roles = "Admin")]
    public override Task<IActionResult> Delete(int id) => base.Delete(id);

    // Endpoint to create a new API key for a sensor (returns plaintext key)
    [HttpPost("{id:int}/apikey")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateApiKey(int id)
    {
        var res = await _apiKeyService.CreateNewApiKey(id);
        if (!res.IsSucceed) return BadRequest(res.ErrorMessage);
        return Ok(new { apiKey = res.Value });
    }

}
