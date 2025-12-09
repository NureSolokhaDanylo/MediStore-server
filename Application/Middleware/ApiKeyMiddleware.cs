using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.Attributes;
using Application.Interfaces;

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
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing API Key");
                return;
            }

            var keyString = key.ToString();
            var authResult = await sensorApiKeyService.AuthenticationAsync(keyString);
            if (!authResult.IsSucceed)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid API Key");
                return;
            }

            // store sensor id in context items for downstream handlers
            context.Items["SensorId"] = authResult.Value;

            await _next(context);
        }
    }

}
