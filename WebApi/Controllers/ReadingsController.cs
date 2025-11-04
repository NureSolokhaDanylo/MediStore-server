using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/readings")]
public class ReadingsController : ControllerBase
{
    private readonly IReadingService _service;
    public ReadingsController(IReadingService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReadingDto>>> GetAll()
        => Ok((await _service.GetAll()).ToDto());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReadingDto>> Get(int id)
    {
        var entity = await _service.Get(id);
        return entity is null ? NotFound() : Ok(entity.ToDto());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReadingDto dto)
    {
        var created = await _service.Add(dto.ToEntity());
        var createdDto = created.ToDto();
        return CreatedAtAction(nameof(Get), new { id = createdDto.Id }, createdDto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ReadingDto>> Update(int id, [FromBody] ReadingDto dto)
    {
        if (id != dto.Id) return BadRequest();
        var updated = await _service.Update(dto.ToEntity());
        return Ok(updated.ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.Delete(id);
        return NoContent();
    }
}
