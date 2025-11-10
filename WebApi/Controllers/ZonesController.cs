using Application.Interfaces;

using Domain.Models;

using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/zones")]
public class ZonesController : CrudController<Zone, ZoneDto, IZoneService>
{
    public ZonesController(IZoneService service) : base(service) { }

    protected override ZoneDto ToDto(Zone entity) => entity.ToDto();
    protected override Zone ToEntity(ZoneDto dto) => dto.ToEntity();
    protected override int GetId(ZoneDto dto) => dto.Id;
}
