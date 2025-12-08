using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    public abstract class MyController : ControllerBase
    {
        public string? userId { get => User.FindFirstValue(ClaimTypes.NameIdentifier); }
        public string? login { get => User.Identity?.Name; }
        public List<string> roles
        {
            get => User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
        }
    }
}
