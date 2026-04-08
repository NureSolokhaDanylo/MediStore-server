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

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Operator")]
    public async Task<ActionResult<BatchDto>> Update(int id, [FromBody] BatchDto dto)
    {
        if (dto.Id != 0 && dto.Id != id)
            return ValidationErrorResult<BatchDto>("Route id and payload id must match.");

        dto.Id = id;
        return await base.Update(dto);
    }

    [Authorize(Roles = "Operator")]
    public override Task<IActionResult> Delete(int id) => base.Delete(id);

    [HttpGet("search")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    public async Task<ActionResult<PagedSearchResultDto<BatchSearchResultDto>>> Search(
        [FromQuery] string q,
        [FromQuery] int? skip = null,
        [FromQuery] int? take = null,
        [FromQuery] int? offset = null,
        [FromQuery] int? limit = null)
    {
        var uid = userId;
        if (string.IsNullOrEmpty(uid)) return UnauthorizedErrorResult<PagedSearchResultDto<BatchSearchResultDto>>();

        var effectiveOffset = skip ?? offset ?? 0;
        var effectiveLimit = take ?? limit ?? 10;

        var result = await _service.SearchByBatchNumber(uid, q, effectiveLimit, effectiveOffset);
        if (!result.IsSucceed) return ApiErrorResult<PagedSearchResultDto<BatchSearchResultDto>>(result);

        var (items, totalCount) = result.Value!;
        return Ok(new PagedSearchResultDto<BatchSearchResultDto>
        {
            Items = items.ToSearchResultDto().ToList(),
            TotalCount = totalCount,
            Limit = effectiveLimit,
            Offset = effectiveOffset
        });
    }
}
