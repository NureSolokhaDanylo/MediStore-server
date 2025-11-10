namespace Domain.Models
{
    public class Batch : EntityBase
    {
        public string BatchNumber { get; set; } = null!;
        public int Quantity { get; set; }
        public DateTime ExpireDate { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;

        //relationships
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; } = null!;
        public int ZoneId { get; set; }
        public Zone Zone { get; set; } = null!;

        public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}
