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
