namespace WebApi.DTOs.AccountDTOs
{
    public class LoginRequestDto
    {
        public required string Login { get; set; } = null!;
        public required string Password { get; set; } = null!;
    }
}
