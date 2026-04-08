using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services
{
    public abstract class ReadOnlyService<T> where T : EntityBase
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
            return entity is null
                ? Result<T>.Failure(new ErrorInfo
                {
                    Code = "common.not_found",
                    Message = "Not found",
                    Type = ErrorType.NotFound,
                    Details = new Dictionary<string, object?> { ["id"] = id }
                })
                : Result<T>.Success(entity);
        }

        public async Task<Result<IEnumerable<T>>> GetAll()
        {
            var list = await _repository.GetAllAsync();
            return Result<IEnumerable<T>>.Success(list);
        }
    }
}
