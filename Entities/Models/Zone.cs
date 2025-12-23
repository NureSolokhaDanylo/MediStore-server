namespace Domain.Models
{
    public class Zone : EntityBase
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; } // NULLABLE
        public double TempMax { get; set; }
        public double TempMin { get; set; }
        public double HumidMax { get; set; }
        public double HumidMin { get; set; }

        //relationships
        public virtual ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
        public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();
        public virtual ICollection<Reading> Readings { get; set; } = new List<Reading>();
    }
}
