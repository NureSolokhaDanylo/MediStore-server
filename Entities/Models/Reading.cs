namespace Domain.Models
{
    public class Reading : EntityBase
    {
        public DateTime TimeStamp { get; set; }
        public double Value { get; set; }

        //relationships
        public int SensorId { get; set; }
        public Sensor Sensor { get; set; } = null!;
    }
}
