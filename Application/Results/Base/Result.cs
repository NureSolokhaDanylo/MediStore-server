namespace Application.Results.Base;

public class Result
{
    public bool IsSucceed { get; private set; }
    public string? ErrorMessage => Error?.Message;
    public ErrorInfo? Error { get; private set; }

    internal Result(bool isSucceed, ErrorInfo? error = null)
    {
        IsSucceed = isSucceed;
        Error = error;
    }

    public static Result Success()
    {
        return new Result(true);
    }

    public static Result Failure(string errorMessage)
    {
        return new Result(false, new ErrorInfo
        {
            Code = "common.unexpected",
            Message = errorMessage,
            Type = ErrorType.Unexpected
        });
    }

    public static Result Failure(ErrorInfo error)
    {
        return new Result(false, error);
    }
}
