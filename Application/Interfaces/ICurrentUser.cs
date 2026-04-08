namespace Application.Interfaces;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    string? Login { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsInRole(string role);
}
