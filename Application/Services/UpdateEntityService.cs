using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.UOW;

namespace Application.Services;

public class UpdateEntityService<T>(
    IUnitOfWork uow,
    IEntityAuditService<T> auditService) : IUpdateService<T> where T : EntityBase
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IEntityAuditService<T> _auditService = auditService;

    public async Task<Result<T>> Update(T entity)
    {
        var repository = _uow.GetRepository<T>();
        var existing = await repository.GetAsync(entity.Id);
        if (existing is null)
            return Result<T>.Failure(Errors.NotFound(ErrorCodes.Common.NotFound, "Not found", "id", entity.Id));

        repository.Update(entity);
        await _uow.SaveChangesAsync();

        await _auditService.LogUpdateAsync(existing, entity);

        return Result<T>.Success(entity);
    }
}
