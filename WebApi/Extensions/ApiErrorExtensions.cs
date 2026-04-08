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
            Code = error.Code.Value,
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
            Code = error.Code.Value,
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
                "not found" => Create(ErrorCodes.Common.NotFound, normalized, ErrorType.NotFound),
                "forbidden" => Create(ErrorCodes.Auth.Forbidden, normalized, ErrorType.Forbidden),
                "invalid credentials" => Create(ErrorCodes.Auth.InvalidCredentials, normalized, ErrorType.Unauthorized),
                "current password required" => Create(ErrorCodes.Auth.CurrentPasswordRequired, normalized, ErrorType.Validation),
                "current password incorrect" => Create(ErrorCodes.Auth.CurrentPasswordIncorrect, normalized, ErrorType.Validation),
                "key is empty" => Create(ErrorCodes.SensorApiKey.EmptyKey, normalized, ErrorType.Unauthorized),
                "invalid api key" => Create(ErrorCodes.SensorApiKey.InvalidKey, normalized, ErrorType.Unauthorized),
                "apikey is not associated with a sensor" => Create(ErrorCodes.SensorApiKey.NotBoundToSensor, normalized, ErrorType.Unauthorized),
                "sensor not found" => Create(ErrorCodes.Sensor.NotFound, normalized, ErrorType.NotFound),
                "requester not found" => Create(ErrorCodes.Account.RequesterNotFound, normalized, ErrorType.NotFound),
                "target user not found" => Create(ErrorCodes.Account.TargetUserNotFound, normalized, ErrorType.NotFound),
                "admin cannot delete themselves" => Create(ErrorCodes.Account.CannotDeleteSelf, normalized, ErrorType.Conflict),
                "cannot change roles of an admin" => Create(ErrorCodes.Account.CannotChangeAdminRoles, normalized, ErrorType.Conflict),
                _ when lower.StartsWith("roles do not exist:") => Create(ErrorCodes.Account.RolesDoNotExist, normalized, ErrorType.Validation),
                _ when lower.Contains("not found") => Create(ErrorCodes.Common.NotFound, normalized, ErrorType.NotFound),
                _ when lower.Contains("forbidden") => Create(ErrorCodes.Auth.Forbidden, normalized, ErrorType.Forbidden),
                _ when lower.Contains("invalid") => Create(ErrorCodes.Common.ValidationError, normalized, ErrorType.Validation),
                _ when lower.Contains("cannot") => Create(ErrorCodes.Common.ValidationError, normalized, ErrorType.Validation),
                _ when lower.Contains("must") => Create(ErrorCodes.Common.ValidationError, normalized, ErrorType.Validation),
                _ when lower.Contains("required") => Create(ErrorCodes.Common.ValidationError, normalized, ErrorType.Validation),
                _ when lower.Contains("out of range") => Create(ErrorCodes.Common.ValidationError, normalized, ErrorType.Validation),
                _ => Create(ErrorCodes.Common.Unexpected, normalized, ErrorType.Unexpected)
            };
        }

        private static ErrorInfo Create(ErrorCode code, string message, ErrorType type)
        {
            return type switch
            {
                ErrorType.Validation => Errors.Validation(code, message),
                ErrorType.Unauthorized => Errors.Unauthorized(code, message),
                ErrorType.Forbidden => Errors.Forbidden(code, message),
                ErrorType.NotFound => Errors.NotFound(code, message),
                ErrorType.Conflict => Errors.Conflict(code, message),
                _ => Errors.Unexpected(code, message)
            };
        }
    }
}
