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
            => Ok((await _service.GetAll()).Select(ToDto));

        [HttpGet("{id:int}")]
        public virtual async Task<ActionResult<TDto>> Get(int id)
        {
            var entity = await _service.Get(id);
            return entity is null ? NotFound() : Ok(ToDto(entity));
        }

        protected abstract TDto ToDto(TEntity entity);
        protected abstract int GetId(TDto dto);
    }
}
