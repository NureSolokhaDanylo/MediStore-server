using Application.Interfaces;

using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class ReadingService : ServiceBase<Reading>, IReadingService
{
    public ReadingService(IReadingRepository repository, IUnitOfWork uow) : base(repository, uow) { }
}
