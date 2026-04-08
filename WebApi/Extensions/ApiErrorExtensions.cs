using System.Text.Json;

using Application.Results.Base;

using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;

namespace WebApi.Extensions;

public static class ApiErrorExtensions
{
    public static IActionResult ToApiErrorResult(this ControllerBase controller, Result result)
    {
        return controller.ToApiErrorResult(result.Error ?? LegacyErrorMapper.FromMessage(result.ErrorMessage));
    }

    public static IActionResult ToApiErrorResult(this ControllerBase controller, ErrorInfo error)
    {
        var status = MapStatus(error.Type);
        var payload = new ApiError
        {
            Code = error.Code,
            Message = error.Message,
            Status = status,
            TraceId = controller.HttpContext.TraceIdentifier,
            Details = error.Details
        };

        return new ObjectResult(payload)
        {
            StatusCode = status
        };
    }

    public static async Task WriteApiErrorAsync(this HttpContext context, ErrorInfo error)
    {
        var status = MapStatus(error.Type);
        var payload = new ApiError
        {
            Code = error.Code,
            Message = error.Message,
            Status = status,
            TraceId = context.TraceIdentifier,
            Details = error.Details
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }

    private static int MapStatus(ErrorType type)
    {
        return type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static class LegacyErrorMapper
    {
        public static ErrorInfo FromMessage(string? message)
        {
            var normalized = (message ?? "Unknown error").Trim();
            var lower = normalized.ToLowerInvariant();

            return lower switch
            {
                "not found" => Create(ApiErrorCodes.Common.NotFound, normalized, ErrorType.NotFound),
                "forbidden" => Create(ApiErrorCodes.Auth.Forbidden, normalized, ErrorType.Forbidden),
                "invalid credentials" => Create(ApiErrorCodes.Auth.InvalidCredentials, normalized, ErrorType.Unauthorized),
                "current password required" => Create(ApiErrorCodes.Auth.CurrentPasswordRequired, normalized, ErrorType.Validation),
                "current password incorrect" => Create(ApiErrorCodes.Auth.CurrentPasswordIncorrect, normalized, ErrorType.Validation),
                "key is empty" => Create(ApiErrorCodes.SensorApiKey.EmptyKey, normalized, ErrorType.Unauthorized),
                "invalid api key" => Create(ApiErrorCodes.SensorApiKey.InvalidKey, normalized, ErrorType.Unauthorized),
                "apikey is not associated with a sensor" => Create(ApiErrorCodes.SensorApiKey.NotBoundToSensor, normalized, ErrorType.Unauthorized),
                "sensor not found" => Create(ApiErrorCodes.Sensor.NotFound, normalized, ErrorType.NotFound),
                "requester not found" => Create(ApiErrorCodes.Account.RequesterNotFound, normalized, ErrorType.NotFound),
                "target user not found" => Create(ApiErrorCodes.Account.TargetUserNotFound, normalized, ErrorType.NotFound),
                "admin cannot delete themselves" => Create(ApiErrorCodes.Account.CannotDeleteSelf, normalized, ErrorType.Conflict),
                "cannot change roles of an admin" => Create(ApiErrorCodes.Account.CannotChangeAdminRoles, normalized, ErrorType.Conflict),
                _ when lower.StartsWith("roles do not exist:") => Create(ApiErrorCodes.Account.RolesDoNotExist, normalized, ErrorType.Validation),
                _ when lower.Contains("not found") => Create(ApiErrorCodes.Common.NotFound, normalized, ErrorType.NotFound),
                _ when lower.Contains("forbidden") => Create(ApiErrorCodes.Auth.Forbidden, normalized, ErrorType.Forbidden),
                _ when lower.Contains("invalid") => Create(ApiErrorCodes.Common.ValidationError, normalized, ErrorType.Validation),
                _ when lower.Contains("cannot") => Create(ApiErrorCodes.Common.ValidationError, normalized, ErrorType.Validation),
                _ when lower.Contains("must") => Create(ApiErrorCodes.Common.ValidationError, normalized, ErrorType.Validation),
                _ when lower.Contains("required") => Create(ApiErrorCodes.Common.ValidationError, normalized, ErrorType.Validation),
                _ when lower.Contains("out of range") => Create(ApiErrorCodes.Common.ValidationError, normalized, ErrorType.Validation),
                _ => Create(ApiErrorCodes.Common.Unexpected, normalized, ErrorType.Unexpected)
            };
        }

        private static ErrorInfo Create(string code, string message, ErrorType type)
        {
            return new ErrorInfo
            {
                Code = code,
                Message = message,
                Type = type
            };
        }
    }
}
