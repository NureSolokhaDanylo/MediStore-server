using System.Text.Json;

using Application.Attributes;
using Application.Interfaces;
using Application.Results.Base;

using Microsoft.AspNetCore.Http;

namespace Application.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string HeaderName = "X-Sensor-Api-Key";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ISensorApiKeyService sensorApiKeyService)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint?.Metadata.GetMetadata<RequireSensorApiKeyAttribute>() == null)
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(HeaderName, out var key))
            {
                await WriteApiErrorAsync(context, Errors.Unauthorized(ErrorCodes.SensorApiKey.EmptyKey, "Missing API Key"));
                return;
            }

            var keyString = key.ToString();
            var authResult = await sensorApiKeyService.AuthenticationAsync(keyString);
            if (!authResult.IsSucceed)
            {
                await WriteApiErrorAsync(context, authResult.Error ?? Errors.Unauthorized(ErrorCodes.SensorApiKey.InvalidKey, "Invalid API Key"));
                return;
            }

            // store sensor id in context items for downstream handlers
            context.Items["SensorId"] = authResult.Value;

            await _next(context);
        }

        private static async Task WriteApiErrorAsync(HttpContext context, ErrorInfo error)
        {
            var status = error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            var payload = new
            {
                error.Code,
                error.Message,
                Status = status,
                TraceId = context.TraceIdentifier,
                error.Details
            };

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }

}
