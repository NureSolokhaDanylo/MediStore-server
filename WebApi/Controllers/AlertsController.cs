using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/alerts")]
public class AlertsController : MyController
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService service)
    {
        _alertService = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operator,Observer")]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetAll()
    {
        var res = await _alertService.GetAll();
        if (!res.IsSucceed) return ApiErrorResult<IEnumerable<AlertDto>>(res);

        var list = res.Value ?? Enumerable.Empty<Domain.Models.Alert>();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    public async Task<ActionResult<AlertDto>> Get(int id)
    {
        var res = await _alertService.Get(id);
        if (!res.IsSucceed) return ApiErrorResult<AlertDto>(res);

        var entity = res.Value;
        if (entity is null) return NotFound();

        return Ok(ToDto(entity));
    }

    [HttpGet("filtered")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    public async Task<ActionResult<PagedResultDto<AlertDto>>> GetFiltered([FromQuery] AlertFilterDto filter)
    {
        var result = await _alertService.GetFilteredAlertsAsync(
            filter.Skip, 
            filter.Take, 
            filter.IsActive, 
            filter.ZoneId, 
            filter.BatchId);

        if (!result.IsSucceed)
            return ApiErrorResult<PagedResultDto<AlertDto>>(result);

        var (items, totalCount) = result.Value!;

        var response = new PagedResultDto<AlertDto>
        {
            Items = items.Select(ToDto),
            TotalCount = totalCount,
            Skip = filter.Skip,
            Take = filter.Take
        };

        return Ok(response);
    }

    private static AlertDto ToDto(Domain.Models.Alert entity) => entity.ToDto();
}
