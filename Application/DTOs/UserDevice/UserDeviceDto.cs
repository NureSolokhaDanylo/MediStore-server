using System;

namespace Application.DTOs.UserDevice
{
    public class UserDeviceDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string FcmToken { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime LastSeenAt { get; set; }
    }
}
