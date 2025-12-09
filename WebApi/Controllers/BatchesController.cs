using Application.Interfaces;

using Domain.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/batches")]
public class BatchesController : CrudController<Batch, BatchDto, BatchCreateDto, IBatchService>
{
    public BatchesController(IBatchService service) : base(service) { }

    protected override BatchDto ToDto(Batch entity) => entity.ToDto();
    protected override Batch ToEntity(BatchDto dto) => dto.ToEntity();
    protected override Batch ToEntity(BatchCreateDto dto) => dto.ToEntity();
    protected override int GetId(BatchDto dto) => dto.Id;

    [Authorize(Roles = "Admin,Operator,Observer")]
    public override Task<ActionResult<IEnumerable<BatchDto>>> GetAll() => base.GetAll();

    [Authorize(Roles = "Admin,Operator,Observer")]
    public override Task<ActionResult<BatchDto>> Get(int id) => base.Get(id);

    [Authorize(Roles = "Operator")]
    public override Task<IActionResult> Create([FromBody] BatchCreateDto dto) => base.Create(dto);

    [Authorize(Roles = "Operator")]
    public override Task<ActionResult<BatchDto>> Update([FromBody] BatchDto dto) => base.Update(dto);

    [Authorize(Roles = "Operator")]
    public override Task<IActionResult> Delete(int id) => base.Delete(id);
}
