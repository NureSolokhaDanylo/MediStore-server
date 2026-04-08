using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.UOW;

namespace Application.Services;

public class CreateEntityService<T>(
    IUnitOfWork uow,
    IEntityAuditService<T> auditService) : ICreateService<T> where T : EntityBase
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IEntityAuditService<T> _auditService = auditService;

    public async Task<Result<T>> Add(T entity)
    {
        await _uow.GetRepository<T>().AddAsync(entity);
        await _uow.SaveChangesAsync();

        await _auditService.LogCreateAsync(entity);

        return Result<T>.Success(entity);
    }
}
