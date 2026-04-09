namespace WebApi.Controllers;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ApiErrorsAttribute(params int[] statusCodes) : Attribute
{
    public IReadOnlyList<int> StatusCodes { get; } = statusCodes;
}
