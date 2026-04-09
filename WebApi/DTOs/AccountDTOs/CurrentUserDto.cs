namespace WebApi.DTOs.AccountDTOs;

public sealed class CurrentUserDto
{
    public required string Id { get; init; }
    public string? Login { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }
}
