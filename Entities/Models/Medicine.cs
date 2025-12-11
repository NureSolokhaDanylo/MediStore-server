namespace Domain.Models
{
    public class Medicine : EntityBase
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; } // NULLABLE
        public double TempMax { get; set; }
        public double TempMin { get; set; }
        public double HumidMax { get; set; }
        public double HumidMin { get; set; }
        public int WarningThresholdDays { get; set; } = 60;

        //relationships
        public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();
    }
}
