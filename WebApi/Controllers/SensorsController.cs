using Application.Interfaces;

using Domain.Models;

using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/sensors")]
public class SensorsController : CrudController<Sensor, SensorDto, ISensorService>
{
    public SensorsController(ISensorService service) : base(service) { }

    protected override SensorDto ToDto(Sensor entity) => entity.ToDto();
    protected override Sensor ToEntity(SensorDto dto) => dto.ToEntity();
    protected override int GetId(SensorDto dto) => dto.Id;
}
