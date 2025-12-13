namespace WebApi.DTOs;

public class UserDto
{
    public required string Id { get; set; }
    public required string UserName { get; set; }
    public string? Email { get; set; }
    public IEnumerable<string>? Roles { get; set; }
}
