using Application.Results.Base;

namespace Application.Interfaces;

public interface IAccessChecker
{
    Result EnsureAuthenticated();
    Result EnsureCurrentUserMatches(string expectedUserId, ErrorInfo? failure = null);
    Result EnsureCurrentUserInRole(string role, ErrorInfo? failure = null);
    Result EnsureCurrentUserInAnyRole(IEnumerable<string> roles, ErrorInfo? failure = null);
}
