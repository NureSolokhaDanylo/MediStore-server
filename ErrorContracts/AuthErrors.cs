namespace Application.Results.Base;

public static class AuthErrors
{
    public static ErrorInfo InvalidCredentials() => Errors.Unauthorized(ErrorCodes.Auth.InvalidCredentials, "Invalid credentials");

    public static ErrorInfo Unauthorized(string message = "Unauthorized") => Errors.Unauthorized(ErrorCodes.Auth.Unauthorized, message);

    public static ErrorInfo Forbidden(string message = "Forbidden") => Errors.Forbidden(ErrorCodes.Auth.Forbidden, message);

    public static ErrorInfo CurrentPasswordRequired() => Errors.Validation(ErrorCodes.Auth.CurrentPasswordRequired, "Current password required");

    public static ErrorInfo CurrentPasswordIncorrect() => Errors.Validation(ErrorCodes.Auth.CurrentPasswordIncorrect, "Current password incorrect");
}
