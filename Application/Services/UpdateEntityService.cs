using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class UpdateEntityService<T>(
    IRepository<T> repository,
    IUnitOfWork uow,
    IEntityAuditService<T> auditService) : IUpdateService<T> where T : EntityBase
{
    private readonly IRepository<T> _repository = repository;
    private readonly IUnitOfWork _uow = uow;
    private readonly IEntityAuditService<T> _auditService = auditService;

    public async Task<Result<T>> Update(string userId, T entity)
    {
        var existing = await _repository.GetAsync(entity.Id);
        if (existing is null)
            return Result<T>.Failure(Errors.NotFound(ErrorCodes.Common.NotFound, "Not found", "id", entity.Id));

        _repository.Update(entity);
        await _uow.SaveChangesAsync();

        await _auditService.LogUpdateAsync(userId, existing, entity);

        return Result<T>.Success(entity);
    }
}
