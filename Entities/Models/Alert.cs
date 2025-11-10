using Domain.Enums;

namespace Domain.Models
{
    public class Alert : EntityBase
    {
        public string Message { get; set; } = null!;
        public bool IsSolved { get; set; } = false;
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public DateTime SolveTime { get; set; }
        public string Signature { get; set; } = null!;
        public AlertType AlertType { get; set; }


        //relationships
        public int? SensorId { get; set; }
        public Sensor? Sensor { get; set; }
        public int? BatchId { get; set; }
        public Batch? Batch { get; set; }
        public int? ZoneId { get; set; }
        public Zone? Zone { get; set; }
    }
}
