using Application.Interfaces;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class BatchService : ServiceBase<Batch>, IBatchService
{
    public BatchService(IBatchRepository repository, IUnitOfWork uow) : base(repository, uow) { }
}
