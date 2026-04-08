namespace Application.Results.Base;

public static class PagingErrors
{
    public static ErrorInfo InvalidSkip(ErrorCode code, string message = "skip cannot be negative")
        => Errors.Validation(code, message, "skip");

    public static ErrorInfo InvalidTake(ErrorCode code, string message = "take must be positive")
        => Errors.Validation(code, message, "take");

    public static ErrorInfo InvalidOffset(ErrorCode code, string message = "Offset cannot be negative")
        => Errors.Validation(code, message, "offset");

    public static ErrorInfo InvalidLimit(ErrorCode code, string message = "Limit must be greater than 0")
        => Errors.Validation(code, message, "limit");

    public static ErrorInfo InvalidCount(ErrorCode code, string message = "Count must be positive")
        => Errors.Validation(code, message, "count");
}
