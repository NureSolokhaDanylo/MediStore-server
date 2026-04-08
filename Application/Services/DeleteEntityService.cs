using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.UOW;

namespace Application.Services;

public class DeleteEntityService<T>(
    IUnitOfWork uow,
    IEntityAuditService<T> auditService) : IDeleteService<T> where T : EntityBase
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IEntityAuditService<T> _auditService = auditService;

    public async Task<Result> Delete(int id)
    {
        var repository = _uow.GetRepository<T>();
        var entity = await repository.GetAsync(id);
        if (entity is null)
            return Result.Failure(Errors.NotFound(ErrorCodes.Common.NotFound, "Not found", "id", id));

        await repository.DeleteAsync(id);
        await _uow.SaveChangesAsync();

        await _auditService.LogDeleteAsync(entity);

        return Result.Success();
    }
}
