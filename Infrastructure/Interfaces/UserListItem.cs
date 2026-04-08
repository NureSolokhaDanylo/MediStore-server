namespace Infrastructure.Interfaces;

public sealed class UserListItem
{
    public required string Id { get; init; }
    public required string UserName { get; init; }
    public string? Email { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; } = [];
}
