using Application.Interfaces;

using Domain.Models;

using Infrastructure.Interfaces;

namespace Application.Services;

public class ReadingService : ServiceBase<Reading>, IReadingService
{
    public ReadingService(IReadingRepository repository) : base(repository) { }
}
