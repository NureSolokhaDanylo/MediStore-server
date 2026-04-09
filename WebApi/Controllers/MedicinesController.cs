using Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/medicines")]
[Consumes("application/json")]
[Produces("application/json")]
public class MedicinesController : MyController
{
    private readonly IMedicineService _service;

    public MedicinesController(IMedicineService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ApiErrors(401, 403)]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ProducesResponseType(typeof(IEnumerable<MedicineDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MedicineDto>>> GetAll()
    {
        var res = await _service.GetAll();
        if (!res.IsSucceed) return ApiErrorResult<IEnumerable<MedicineDto>>(res);

        var list = res.Value ?? Enumerable.Empty<Domain.Models.Medicine>();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ApiErrors(401, 403, 404)]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ApiErrorCodes(404, "medicine.not_found")]
    [ProducesResponseType(typeof(MedicineDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MedicineDto>> Get(int id)
    {
        var res = await _service.Get(id);
        if (!res.IsSucceed) return ApiErrorResult<MedicineDto>(res);

        var entity = res.Value;
        if (entity is null) return NotFound();

        return Ok(ToDto(entity));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ApiErrors(400, 401, 403)]
    [ApiErrorCodes(400, "medicine.temp_min_out_of_range", "medicine.temp_max_out_of_range", "medicine.temp_range_invalid", "medicine.humid_min_out_of_range", "medicine.humid_max_out_of_range", "medicine.humid_range_invalid")]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ProducesResponseType(typeof(MedicineDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] MedicineCreateDto dto)
    {
        var res = await _service.Add(dto.ToEntity());
        if (!res.IsSucceed) return ApiErrorResult(res);

        var createdDto = ToDto(res.Value!);
        return CreatedAtAction(nameof(Get), new { id = createdDto.Id }, createdDto);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    [ApiErrors(400, 401, 403, 404)]
    [ApiErrorCodes(400, "medicine.temp_min_out_of_range", "medicine.temp_max_out_of_range", "medicine.temp_range_invalid", "medicine.humid_min_out_of_range", "medicine.humid_max_out_of_range", "medicine.humid_range_invalid")]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ApiErrorCodes(404, "medicine.not_found")]
    [ProducesResponseType(typeof(MedicineDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MedicineDto>> Update([FromBody] MedicineDto dto)
    {
        var res = await _service.Update(dto.ToEntity());
        if (!res.IsSucceed) return ApiErrorResult<MedicineDto>(res);

        return Ok(ToDto(res.Value!));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ApiErrors(400, 401, 403, 404)]
    [ApiErrorCodes(400, "common.validation_error", "medicine.temp_min_out_of_range", "medicine.temp_max_out_of_range", "medicine.temp_range_invalid", "medicine.humid_min_out_of_range", "medicine.humid_max_out_of_range", "medicine.humid_range_invalid")]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ApiErrorCodes(404, "medicine.not_found")]
    [ProducesResponseType(typeof(MedicineDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MedicineDto>> Update(int id, [FromBody] MedicineDto dto)
    {
        if (dto.Id != 0 && dto.Id != id)
            return ValidationErrorResult<MedicineDto>("Route id and payload id must match.");

        dto.Id = id;
        return await Update(dto);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ApiErrors(401, 403, 404)]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ApiErrorCodes(404, "medicine.not_found")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _service.Delete(id);
        if (!res.IsSucceed) return ApiErrorResult(res);

        return NoContent();
    }

    [HttpGet("search")]
    [Authorize(Roles = "Admin,Operator,Observer")]
    [ApiErrors(400, 401, 403)]
    [ApiErrorCodes(400, "medicine.invalid_search_paging")]
    [ApiErrorCodes(401, "auth.unauthorized")]
    [ApiErrorCodes(403, "auth.forbidden")]
    [ProducesResponseType(typeof(PagedSearchResultDto<MedicineSearchResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedSearchResultDto<MedicineSearchResultDto>>> Search(
        [FromQuery] string q,
        [FromQuery] int? skip = null,
        [FromQuery] int? take = null,
        [FromQuery] int? offset = null,
        [FromQuery] int? limit = null)
    {
        var effectiveOffset = skip ?? offset ?? 0;
        var effectiveLimit = take ?? limit ?? 10;

        var result = await _service.Search(q, effectiveLimit, effectiveOffset);
        if (!result.IsSucceed) return ApiErrorResult<PagedSearchResultDto<MedicineSearchResultDto>>(result);

        var (items, totalCount) = result.Value!;
        return Ok(new PagedSearchResultDto<MedicineSearchResultDto>
        {
            Items = items.ToSearchResultDto().ToList(),
            TotalCount = totalCount,
            Limit = effectiveLimit,
            Offset = effectiveOffset
        });
    }

    private static MedicineDto ToDto(Domain.Models.Medicine entity) => entity.ToDto();
}
