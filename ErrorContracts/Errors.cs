namespace Application.Results.Base;

public static class Errors
{
    public static ErrorInfo Validation(ErrorCode code, string message, string? field = null, Dictionary<string, object?>? details = null)
        => Create(ErrorType.Validation, code, message, field, details);

    public static ErrorInfo Unauthorized(ErrorCode code, string message, Dictionary<string, object?>? details = null)
        => Create(ErrorType.Unauthorized, code, message, null, details);

    public static ErrorInfo Forbidden(ErrorCode code, string message, Dictionary<string, object?>? details = null)
        => Create(ErrorType.Forbidden, code, message, null, details);

    public static ErrorInfo NotFound(ErrorCode code, string message, string? key = null, object? value = null, Dictionary<string, object?>? details = null)
    {
        var payload = details is null ? null : new Dictionary<string, object?>(details);
        if (!string.IsNullOrWhiteSpace(key))
        {
            payload ??= new Dictionary<string, object?>();
            payload[key] = value;
        }

        return new ErrorInfo
        {
            Code = code,
            Message = message,
            Type = ErrorType.NotFound,
            Details = payload
        };
    }

    public static ErrorInfo Conflict(ErrorCode code, string message, Dictionary<string, object?>? details = null)
        => Create(ErrorType.Conflict, code, message, null, details);

    public static ErrorInfo Unexpected(ErrorCode code, string message, Dictionary<string, object?>? details = null)
        => Create(ErrorType.Unexpected, code, message, null, details);

    private static ErrorInfo Create(ErrorType type, ErrorCode code, string message, string? field, Dictionary<string, object?>? details)
    {
        var payload = details is null ? null : new Dictionary<string, object?>(details);
        if (!string.IsNullOrWhiteSpace(field))
        {
            payload ??= new Dictionary<string, object?>();
            payload["field"] = field;
        }

        return new ErrorInfo
        {
            Code = code,
            Message = message,
            Type = type,
            Details = payload
        };
    }
}
