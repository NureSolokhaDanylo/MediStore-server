using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.UserDevice
{
    public class CreateUserDeviceDto
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public string FcmToken { get; set; } = null!;
    }
}
