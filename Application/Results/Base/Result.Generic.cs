namespace Application.Results.Base;

public sealed class Result<T> : Result
{
    public T? Value { get; private set; }

    private Result(bool isSucceed, T? value, ErrorInfo? error = null) : base(isSucceed, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value);
    }

    public new static Result<T> Failure(string errorMessage)
    {
        return new Result<T>(false, default, new ErrorInfo
        {
            Code = "common.unexpected",
            Message = errorMessage,
            Type = ErrorType.Unexpected
        });
    }

    public new static Result<T> Failure(ErrorInfo error)
    {
        return new Result<T>(false, default, error);
    }
}
