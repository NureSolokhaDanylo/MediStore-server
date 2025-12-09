using Application.Interfaces;

using Domain.Models;

using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

// Full CRUD controller: inherits read operations and adds create/update/delete
public abstract class CrudController<TEntity, TDto, TCreateDto, TService> : ReadController<TEntity, TDto, TService>
    where TEntity : EntityBase
    where TService : IService<TEntity>
{
    protected CrudController(TService service) : base(service) { }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] TCreateDto dto)
    {
        var created = await _service.Add(ToEntity(dto));
        var createdDto = ToDto(created);
        return CreatedAtAction(nameof(Get), new { id = GetId(createdDto) }, createdDto);
    }

    [HttpPut]
    public virtual async Task<ActionResult<TDto>> Update([FromBody] TDto dto)
    {
        var updated = await _service.Update(ToEntity(dto));
        return Ok(ToDto(updated));
    }

    [HttpDelete("{id:int}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        await _service.Delete(id);
        return NoContent();
    }

    protected abstract TEntity ToEntity(TDto dto);
    protected abstract TEntity ToEntity(TCreateDto dto);
}
