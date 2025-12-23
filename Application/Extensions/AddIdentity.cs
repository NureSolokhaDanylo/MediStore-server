using Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class IdentityExtensions
{
    public static IServiceCollection AddAppIdentity(this IServiceCollection services)
    {
        return services.AddInfrastructureIdentity();
    }
}
