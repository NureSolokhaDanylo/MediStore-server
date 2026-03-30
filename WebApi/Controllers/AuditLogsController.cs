using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/audit-logs")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : MyController
{
    private readonly IAuditLogService _service;
    public AuditLogsController(IAuditLogService service)
    {
        _service = service;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var res = await _service.GetByIdAsync(id);
        if (!res.IsSucceed) return NotFound(res.ErrorMessage);
        if (res.Value is null) return NotFound();
        return Ok(res.Value.ToDto());
    }

    [HttpGet("type/{entityType}")]
    public async Task<IActionResult> GetByTypeRange([FromRoute] string entityType, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        if (string.IsNullOrWhiteSpace(entityType)) return BadRequest("entityType is required");

        var res = await _service.GetByTypeAsync(entityType, from?.ToUniversalTime(), to?.ToUniversalTime(), null);
        if (!res.IsSucceed) return BadRequest(res.ErrorMessage);

        var list = res.Value ?? Enumerable.Empty<Domain.Models.AuditLog>();
        return Ok(list.ToDto());
    }

    [HttpGet("type/{entityType}/paged")]
    public async Task<ActionResult<PagedResultDto<AuditLogDto>>> GetByTypePaged(
        [FromRoute] string entityType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        if (string.IsNullOrWhiteSpace(entityType)) return BadRequest("entityType is required");
        if (skip < 0) return BadRequest("skip cannot be negative");
        if (take <= 0) return BadRequest("take must be positive");

        var res = await _service.GetByTypePagedAsync(entityType, from?.ToUniversalTime(), to?.ToUniversalTime(), skip, take);
        if (!res.IsSucceed) return BadRequest(res.ErrorMessage);

        var (items, totalCount) = res.Value!;
        return Ok(new PagedResultDto<AuditLogDto>
        {
            Items = items.ToDto(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        });
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PagedResultDto<AuditLogDto>>> GetPaged(
        [FromQuery] string? q = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? action = null,
        [FromQuery] string? userId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        if (skip < 0) return BadRequest("skip cannot be negative");
        if (take <= 0) return BadRequest("take must be positive");

        var res = await _service.GetPagedAsync(
            q,
            entityType,
            action,
            userId,
            from?.ToUniversalTime(),
            to?.ToUniversalTime(),
            skip,
            take);

        if (!res.IsSucceed) return BadRequest(res.ErrorMessage);

        var (items, totalCount) = res.Value!;
        return Ok(new PagedResultDto<AuditLogDto>
        {
            Items = items.ToDto(),
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        });
    }

    [HttpGet("type/{entityType}/last")]
    public async Task<IActionResult> GetByTypeLast([FromRoute] string entityType, [FromQuery] int count)
    {
        if (string.IsNullOrWhiteSpace(entityType)) return BadRequest("entityType is required");
        if (count <= 0) return BadRequest("count must be positive");

        var res = await _service.GetByTypeAsync(entityType, null, null, count);
        if (!res.IsSucceed) return BadRequest(res.ErrorMessage);

        var list = res.Value ?? Enumerable.Empty<Domain.Models.AuditLog>();
        return Ok(list.ToDto());
    }
}
