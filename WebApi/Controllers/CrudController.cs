using Application.Interfaces;

using Domain.Models;

using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public abstract class CrudController<TEntity, TDto, TService> : MyController
 where TEntity : EntityBase
 where TService : IService<TEntity>
{
    protected readonly TService _service;
    protected CrudController(TService service) => _service = service;

    [HttpGet]
    public virtual async Task<ActionResult<IEnumerable<TDto>>> GetAll()
    => Ok((await _service.GetAll()).Select(ToDto));

    [HttpGet("{id:int}")]
    public virtual async Task<ActionResult<TDto>> Get(int id)
    {
        var entity = await _service.Get(id);
        return entity is null ? NotFound() : Ok(ToDto(entity));
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] TDto dto)
    {
        var created = await _service.Add(ToEntity(dto));
        var createdDto = ToDto(created);
        return CreatedAtAction(nameof(Get), new { id = GetId(createdDto) }, createdDto);
    }

    [HttpPut("{id:int}")]
    public virtual async Task<ActionResult<TDto>> Update(int id, [FromBody] TDto dto)
    {
        if (id != GetId(dto)) return BadRequest();
        var updated = await _service.Update(ToEntity(dto));
        return Ok(ToDto(updated));
    }

    [HttpDelete("{id:int}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        await _service.Delete(id);
        return NoContent();
    }

    protected abstract TDto ToDto(TEntity entity);
    protected abstract TEntity ToEntity(TDto dto);
    protected abstract int GetId(TDto dto);
}
