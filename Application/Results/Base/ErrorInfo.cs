namespace Application.Results.Base;

public sealed class ErrorInfo
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public ErrorType Type { get; init; }
    public Dictionary<string, object?>? Details { get; init; }
}
