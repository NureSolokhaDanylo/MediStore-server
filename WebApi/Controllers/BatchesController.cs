using Application.Interfaces;

using Domain.Models;

using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/batches")]
public class BatchesController : CrudController<Batch, BatchDto, IBatchService>
{
    public BatchesController(IBatchService service) : base(service) { }

    protected override BatchDto ToDto(Batch entity) => entity.ToDto();
    protected override Batch ToEntity(BatchDto dto) => dto.ToEntity();
    protected override int GetId(BatchDto dto) => dto.Id;
}
