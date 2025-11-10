using Application.Interfaces;

using Domain.Models;

using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/readings")]
public class ReadingsController : CrudController<Reading, ReadingDto, IReadingService>
{
    public ReadingsController(IReadingService service) : base(service) { }

    protected override ReadingDto ToDto(Reading entity) => entity.ToDto();
    protected override Reading ToEntity(ReadingDto dto) => dto.ToEntity();
    protected override int GetId(ReadingDto dto) => dto.Id;
}
