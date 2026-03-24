using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class UserDevice : EntityBase
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public string FcmToken { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime LastSeenAt { get; set; }
    }
}
