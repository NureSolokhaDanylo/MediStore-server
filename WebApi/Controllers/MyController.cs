using System.Security.Claims;

using Application.Results.Base;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

using WebApi.DTOs;
using WebApi.Extensions;

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

        public int? sensorId { get => HttpContext.Items["SensorId"] as int?; }

        protected IActionResult ApiErrorResult(Result result) => this.ToApiErrorResult(result);

        protected IActionResult ApiErrorResult(ErrorInfo error) => this.ToApiErrorResult(error);

        protected ActionResult<T> ApiErrorResult<T>(Result result) => (ObjectResult)ApiErrorResult(result);

        protected ActionResult<T> ApiErrorResult<T>(ErrorInfo error) => (ObjectResult)ApiErrorResult(error);

        protected IActionResult ValidationErrorResult(string message, ErrorCode? code = null)
            => ApiErrorResult(Errors.Validation(code ?? ErrorCodes.Common.ValidationError, message));

        protected ActionResult<T> ValidationErrorResult<T>(string message, ErrorCode? code = null)
            => (ObjectResult)ValidationErrorResult(message, code);

        protected IActionResult UnauthorizedErrorResult(string message = "Unauthorized", ErrorCode? code = null)
            => ApiErrorResult(Errors.Unauthorized(code ?? ErrorCodes.Auth.Unauthorized, message));

        protected ActionResult<T> UnauthorizedErrorResult<T>(string message = "Unauthorized", ErrorCode? code = null)
            => (ObjectResult)UnauthorizedErrorResult(message, code);
    }
}
