namespace WebApi.DTOs.AccountDTOs
{
    public class CreateAccountDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public IEnumerable<string>? Roles { get; set; }
    }
}
