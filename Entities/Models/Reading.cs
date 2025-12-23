namespace Domain.Models
{
    public class Reading : EntityBase
    {
        public DateTime TimeStamp { get; set; }
        public double Value { get; set; }

        //relationships
        public int? SensorId { get; set; }
        public virtual Sensor? Sensor { get; set; } = null!;

        public int? ZoneId { get; set; }
        public virtual Zone? Zone { get; set; } = null!;
    }
}
