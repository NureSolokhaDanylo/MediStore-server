namespace Domain.Models
{
    public class Batch : EntityBase
    {
        public string BatchNumber { get; set; } = null!;
        public int Quantity { get; set; }
        public DateTime ExpireDate { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        //relationships
        public int MedicineId { get; set; }
        public virtual Medicine Medicine { get; set; } = null!;
        public int ZoneId { get; set; }
        public virtual Zone Zone { get; set; } = null!;

        public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}
