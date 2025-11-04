using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
