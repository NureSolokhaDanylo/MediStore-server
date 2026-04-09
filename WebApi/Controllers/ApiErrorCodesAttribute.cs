namespace WebApi.Controllers;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class ApiErrorCodesAttribute(int statusCode, params string[] codes) : Attribute
{
    public int StatusCode { get; } = statusCode;
    public IReadOnlyList<string> Codes { get; } = codes;
}
