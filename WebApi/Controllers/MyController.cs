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

        protected IActionResult ValidationErrorResult(string message, string code = ApiErrorCodes.Common.ValidationError)
            => ApiErrorResult(new ErrorInfo
            {
                Code = code,
                Message = message,
                Type = ErrorType.Validation
            });

        protected ActionResult<T> ValidationErrorResult<T>(string message, string code = ApiErrorCodes.Common.ValidationError)
            => (ObjectResult)ValidationErrorResult(message, code);

        protected IActionResult UnauthorizedErrorResult(string message = "Unauthorized", string code = ApiErrorCodes.Auth.Unauthorized)
            => ApiErrorResult(new ErrorInfo
            {
                Code = code,
                Message = message,
                Type = ErrorType.Unauthorized
            });

        protected ActionResult<T> UnauthorizedErrorResult<T>(string message = "Unauthorized", string code = ApiErrorCodes.Auth.Unauthorized)
            => (ObjectResult)UnauthorizedErrorResult(message, code);
    }
}
