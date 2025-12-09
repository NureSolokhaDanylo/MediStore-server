using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Models
{
    public class SensorApiKey : EntityBase
    {
        public string ApiKeyHash { get; set; } = default!;
        public bool IsActive { get; set; } = true;

        //relationships
        public int? SensorId { get; set; }
        public virtual Sensor? Sensor { get; set; }
    }
}
