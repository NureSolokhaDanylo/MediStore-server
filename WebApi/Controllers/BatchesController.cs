using Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/batches")]
[Consumes("application/json")]
[Produces("application/json")]
public class BatchesController : MyController
{
    private readonly IBatchService _service;

    public BatchesController(IBatchService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ProducesResponseType(typeof(IEnumerable<BatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BatchDto>>> GetAll()
    {
        var res = await _service.GetAll();
        if (!res.IsSucceed) return ApiErrorResult<IEnumerable<BatchDto>>(res);

        var list = res.Value ?? Enumerable.Empty<Domain.Models.Batch>();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ProducesResponseType(typeof(BatchDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BatchDto>> Get(int id)
    {
        var res = await _service.Get(id);
        if (!res.IsSucceed) return ApiErrorResult<BatchDto>(res);

        var entity = res.Value;
        if (entity is null) return NotFound();

        return Ok(ToDto(entity));
    }

    [HttpPost]
    [Authorize(Roles = "Operator")]
    [ProducesResponseType(typeof(BatchDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] BatchCreateDto dto)
    {
        var res = await _service.Add(dto.ToEntity());
        if (!res.IsSucceed) return ApiErrorResult(res);

        var createdDto = ToDto(res.Value!);
        return CreatedAtAction(nameof(Get), new { id = createdDto.Id }, createdDto);
    }

    [HttpPut]
    [Authorize(Roles = "Operator")]
    [ProducesResponseType(typeof(BatchDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BatchDto>> Update([FromBody] BatchDto dto)
    {
        var res = await _service.Update(dto.ToEntity());
        if (!res.IsSucceed) return ApiErrorResult<BatchDto>(res);

        return Ok(ToDto(res.Value!));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Operator")]
    [ProducesResponseType(typeof(BatchDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BatchDto>> Update(int id, [FromBody] BatchDto dto)
    {
        if (dto.Id != 0 && dto.Id != id)
            return ValidationErrorResult<BatchDto>("Route id and payload id must match.");

        dto.Id = id;
        return await Update(dto);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Operator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _service.Delete(id);
        if (!res.IsSucceed) return ApiErrorResult(res);

        return NoContent();
    }

    [HttpGet("search")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ProducesResponseType(typeof(PagedSearchResultDto<BatchSearchResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedSearchResultDto<BatchSearchResultDto>>> Search(
        [FromQuery] string q,
        [FromQuery] int? skip = null,
        [FromQuery] int? take = null,
        [FromQuery] int? offset = null,
        [FromQuery] int? limit = null)
    {
        var effectiveOffset = skip ?? offset ?? 0;
        var effectiveLimit = take ?? limit ?? 10;

        var result = await _service.SearchByBatchNumber(q, effectiveLimit, effectiveOffset);
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

    private static BatchDto ToDto(Domain.Models.Batch entity) => entity.ToDto();
}
