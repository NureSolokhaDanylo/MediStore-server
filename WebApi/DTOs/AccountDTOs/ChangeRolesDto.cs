namespace WebApi.DTOs.AccountDTOs
{
    public class ChangeRolesDto
    {
        public required string TargetUserId { get; set; }
        public required IEnumerable<string> Roles { get; set; }
    }
}
