using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class DeleteEntityService<T>(
    IRepository<T> repository,
    IUnitOfWork uow,
    IEntityAuditService<T> auditService) : IDeleteService<T> where T : EntityBase
{
    private readonly IRepository<T> _repository = repository;
    private readonly IUnitOfWork _uow = uow;
    private readonly IEntityAuditService<T> _auditService = auditService;

    public async Task<Result> Delete(string userId, int id)
    {
        var entity = await _repository.GetAsync(id);
        if (entity is null)
            return Result.Failure(Errors.NotFound(ErrorCodes.Common.NotFound, "Not found", "id", id));

        await _repository.DeleteAsync(id);
        await _uow.SaveChangesAsync();

        await _auditService.LogDeleteAsync(userId, entity);

        return Result.Success();
    }
}
