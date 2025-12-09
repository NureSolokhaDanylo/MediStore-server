namespace WebApi.DTOs.AccountDTOs
{
    public class ChangePasswordDto
    {
        public string? TargetUserId { get; set; }
        public string? CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}
