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
    public ZonesController(IZoneService service) : base(service) { }

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
}
