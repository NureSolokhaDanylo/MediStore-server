using Application.Interfaces;
using Domain.Models;
using Infrastructure.Interfaces;

namespace Application.Services;

public class BatchService : ServiceBase<Batch>, IBatchService
{
 public BatchService(IBatchRepository repository) : base(repository) { }
}
