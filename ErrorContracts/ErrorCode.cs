namespace Application.Results.Base;

public sealed record ErrorCode
{
    public string Value { get; }

    private ErrorCode(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;

    internal static ErrorCode Create(string value) => new(value);
}
