using Application.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/sensors")]
public class SensorsController : CrudController<Sensor, SensorDto, SensorCreateDto, ISensorService>
{
    public SensorsController(ISensorService service) : base(service) { }

    protected override SensorDto ToDto(Sensor entity) => entity.ToDto();
    protected override Sensor ToEntity(SensorDto dto) => dto.ToEntity();
    protected override Sensor ToEntity(SensorCreateDto dto) => dto.ToEntity();
    protected override int GetId(SensorDto dto) => dto.Id;

    [NonAction]
    //понадобится всем
    public override Task<ActionResult<IEnumerable<SensorDto>>> GetAll() => base.GetAll();

    [NonAction]
    //понадобится всем
    public override Task<ActionResult<SensorDto>> Get(int id) => base.Get(id);

    [NonAction]
    //понадобится админу
    public override Task<IActionResult> Create([FromBody] SensorCreateDto dto) => base.Create(dto);

    [NonAction]
    //понадобится админу
    public override Task<ActionResult<SensorDto>> Update([FromBody] SensorDto dto) => base.Update(dto);

    [NonAction]
    //понадобится админу
    public override Task<IActionResult> Delete(int id) => base.Delete(id);
}
