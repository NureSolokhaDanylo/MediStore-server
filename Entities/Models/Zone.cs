using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Enums;

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
        public ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
        public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    }
}
