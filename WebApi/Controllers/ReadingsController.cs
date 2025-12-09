using Application.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/readings")]
public class ReadingsController : CrudController<Reading, ReadingDto, ReadingCreateDto, IReadingService>
{
    public ReadingsController(IReadingService service) : base(service) {}

    protected override ReadingDto ToDto(Reading entity) => entity.ToDto();
    protected override Reading ToEntity(ReadingDto dto) => dto.ToEntity();
    protected override Reading ToEntity(ReadingCreateDto dto) => dto.ToEntity();
    protected override int GetId(ReadingDto dto) => dto.Id;

    [NonAction]
    // понадобится всем
    public override Task<ActionResult<IEnumerable<ReadingDto>>> GetAll() => base.GetAll();

    [NonAction]
    // понадобится всем
    public override Task<ActionResult<ReadingDto>> Get(int id) => base.Get(id);

    [NonAction]
    // понадобится только сенсорам
    public override Task<IActionResult> Create([FromBody] ReadingCreateDto dto) => base.Create(dto);

    [NonAction]
    //не понадобится никогда
    public override Task<ActionResult<ReadingDto>> Update([FromBody] ReadingDto dto) => base.Update(dto);

    [NonAction]
    // скорее всего не понадобится никогда
    public override Task<IActionResult> Delete(int id) => base.Delete(id);
}
