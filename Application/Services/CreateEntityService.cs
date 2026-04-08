using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class CreateEntityService<T>(
    IRepository<T> repository,
    IUnitOfWork uow,
    IEntityAuditService<T> auditService) : ICreateService<T> where T : EntityBase
{
    private readonly IRepository<T> _repository = repository;
    private readonly IUnitOfWork _uow = uow;
    private readonly IEntityAuditService<T> _auditService = auditService;

    public async Task<Result<T>> Add(string userId, T entity)
    {
        await _repository.AddAsync(entity);
        await _uow.SaveChangesAsync();

        await _auditService.LogCreateAsync(userId, entity);

        return Result<T>.Success(entity);
    }
}
