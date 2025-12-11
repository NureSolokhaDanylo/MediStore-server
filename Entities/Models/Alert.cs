using Domain.Enums;

namespace Domain.Models
{
    public class Alert : EntityBase
    {
        public string Message { get; set; } = null!;
        public DateTime CreationTime { get; set; } = DateTime.UtcNow;
        public AlertType AlertType { get; set; }


        //relationships
        public int? SensorId { get; set; }
        public virtual Sensor? Sensor { get; set; }
        public int? BatchId { get; set; }
        public virtual Batch? Batch { get; set; }
        public int? ZoneId { get; set; }
        public virtual Zone? Zone { get; set; }
    }
}
