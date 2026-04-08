using System.Security.Claims;

using Application.Interfaces;

using Microsoft.AspNetCore.Http;

namespace WebApi.Services;

public class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Login => User?.Identity?.Name;

    public IReadOnlyCollection<string> Roles =>
        User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray()
        ?? [];

    public bool IsInRole(string role) =>
        Roles.Contains(role, StringComparer.Ordinal);
}
