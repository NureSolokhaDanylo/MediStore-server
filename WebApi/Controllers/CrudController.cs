using Application.Interfaces;

using Domain.Models;

using Microsoft.AspNetCore.Mvc;

using System.IO;

namespace WebApi.Controllers;

// Full CRUD controller: inherits read operations and adds create/update/delete
public abstract class CrudController<TEntity, TDto, TCreateDto, TService> : ReadController<TEntity, TDto, TService>
    where TEntity : EntityBase
    where TService : ICrudService<TEntity>
{
    protected CrudController(TService service) : base(service) { }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] TCreateDto dto)
    {
        var uid = userId;
        if (string.IsNullOrEmpty(uid)) return UnauthorizedErrorResult();

        var res = await _service.Add(uid, ToEntity(dto));
        if (!res.IsSucceed) return ApiErrorResult(res);
        var created = res.Value!;
        var createdDto = ToDto(created);
        return CreatedAtAction(nameof(Get), new { id = GetId(createdDto) }, createdDto);
    }

    [HttpPut]
    public virtual async Task<ActionResult<TDto>> Update([FromBody] TDto dto)
    {
        var uid = userId;
        if (string.IsNullOrEmpty(uid)) return UnauthorizedErrorResult<TDto>();

        var res = await _service.Update(uid, ToEntity(dto));
        if (!res.IsSucceed) return ApiErrorResult<TDto>(res);
        var updated = res.Value!;
        return Ok(ToDto(updated));
    }

    [HttpDelete("{id:int}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var uid = userId;
        if (string.IsNullOrEmpty(uid)) return UnauthorizedErrorResult();

        var res = await _service.Delete(uid, id);
        if (!res.IsSucceed) return ApiErrorResult(res);
        return NoContent();
    }

    protected abstract TEntity ToEntity(TDto dto);
    protected abstract TEntity ToEntity(TCreateDto dto);
}
