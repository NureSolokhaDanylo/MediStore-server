using Application.Interfaces;

using Domain.Models;

using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    // Read-only controller: exposes GET and GET ALL
    public abstract class ReadController<TEntity, TDto, TService> : MyController
        where TEntity : EntityBase
        where TService : IService<TEntity>
    {
        protected readonly TService _service;
        protected ReadController(TService service) => _service = service;

        [HttpGet]
        public virtual async Task<ActionResult<IEnumerable<TDto>>> GetAll()
        {
            var res = await _service.GetAll();
            if (!res.IsSucceed) return BadRequest(res.ErrorMessage);
            var list = res.Value ?? Enumerable.Empty<TEntity>();
            return Ok(list.Select(ToDto));
        }

        [HttpGet("{id:int}")]
        public virtual async Task<ActionResult<TDto>> Get(int id)
        {
            var res = await _service.Get(id);
            if (!res.IsSucceed) return NotFound(res.ErrorMessage);
            var entity = res.Value;
            if (entity is null) return NotFound();
            return Ok(ToDto(entity));
        }

        protected abstract TDto ToDto(TEntity entity);
        protected abstract int GetId(TDto dto);
    }
}
