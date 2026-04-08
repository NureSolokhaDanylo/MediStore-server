using Application.Interfaces;
using Application.Results.Base;

namespace Application.Services;

public class AccessChecker(ICurrentUser currentUser) : IAccessChecker
{
    private readonly ICurrentUser _currentUser = currentUser;

    public Result EnsureAuthenticated()
    {
        if (_currentUser.IsAuthenticated && !string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            return Result.Success();
        }

        return Result.Failure(AuthErrors.Unauthorized());
    }

    public Result EnsureCurrentUserMatches(string expectedUserId, ErrorInfo? failure = null)
    {
        var auth = EnsureAuthenticated();
        if (!auth.IsSucceed)
        {
            return auth;
        }

        if (string.Equals(_currentUser.UserId, expectedUserId, StringComparison.Ordinal))
        {
            return Result.Success();
        }

        return Result.Failure(failure ?? AuthErrors.Forbidden());
    }

    public Result EnsureCurrentUserInRole(string role, ErrorInfo? failure = null)
    {
        var auth = EnsureAuthenticated();
        if (!auth.IsSucceed)
        {
            return auth;
        }

        if (_currentUser.IsInRole(role))
        {
            return Result.Success();
        }

        return Result.Failure(failure ?? AuthErrors.Forbidden());
    }
}
