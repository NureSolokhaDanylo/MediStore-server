using Domain.Enums;

namespace Domain.Models
{
    public class Sensor : EntityBase
    {
        public string SerialNumber { get; set; } = null!;
        public double? LastValue { get; set; } // NULLABLE
        public DateTime? LastUpdate { get; set; } // NULLABLE
        public bool IsOn { get; set; }
        public SensorType SensorType { get; set; }

        //relationships
        public int? ZoneId { get; set; }
        public virtual Zone? Zone { get; set; }

        public virtual ICollection<Reading> Readings { get; set; } = new List<Reading>();
        public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}
