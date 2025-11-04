using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Zone? Zone { get; set; }

        public ICollection<Reading> Readings { get; set; } = new List<Reading>();
        public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}
