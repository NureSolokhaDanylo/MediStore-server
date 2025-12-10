using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.Results.Base;

using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services
{
    public class ReadOnlyService<T> where T : EntityBase
    {
        protected readonly IRepository<T> _repository;
        protected readonly IUnitOfWork _uow;

        public ReadOnlyService(IRepository<T> repository, IUnitOfWork uow)
        {
            _repository = repository;
            _uow = uow;
        }

        public async Task<Result<T>> Get(int id)
        {
            var entity = await _repository.GetAsync(id);
            return entity is null ? Result<T>.Failure("Not found") : Result<T>.Success(entity);
        }

        public async Task<Result<IEnumerable<T>>> GetAll()
        {
            var list = await _repository.GetAllAsync();
            return Result<IEnumerable<T>>.Success(list);
        }
    }
}
