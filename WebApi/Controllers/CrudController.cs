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
        var res = await _service.Add(ToEntity(dto));
        if (!res.IsSucceed) return BadRequest(res.ErrorMessage);
        var created = res.Value!;
        var createdDto = ToDto(created);
        return CreatedAtAction(nameof(Get), new { id = GetId(createdDto) }, createdDto);
    }

    [HttpPut]
    public virtual async Task<ActionResult<TDto>> Update([FromBody] TDto dto)
    {
        var res = await _service.Update(ToEntity(dto));
        if (!res.IsSucceed) return NotFound(res.ErrorMessage);
        var updated = res.Value!;
        return Ok(ToDto(updated));
    }

    [HttpDelete("{id:int}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var res = await _service.Delete(id);
        if (!res.IsSucceed) return NotFound(res.ErrorMessage);
        return NoContent();
    }

    protected abstract TEntity ToEntity(TDto dto);
    protected abstract TEntity ToEntity(TCreateDto dto);
}
