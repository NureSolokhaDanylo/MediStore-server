using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class AuditLog : EntityBase
    {
        public DateTime OccurredAt { get; set; }

        public string EntityType { get; set; } = null!;   // "Sensor", "Batch"
        public int EntityId { get; set; }

        public string Action { get; set; } = null!;       // Create / Update / Delete

        public string? UserId { get; set; }                // если есть автор

        public string? Summary { get; set; }               // коротко, человеко-читаемо

        public string? OldValues { get; set; }             // JSON
        public string? NewValues { get; set; }             // JSON
    }

}
