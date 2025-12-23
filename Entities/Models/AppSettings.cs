using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class AppSettings : EntityBase
    {
        public bool AlertEnabled { get; set; } = true;
        public double TempAlertDeviation { get; set; } = 2.0;
        public double HumidityAlertDeviation { get; set; } = 5.0;
        public TimeSpan CheckDeviationInterval { get; set; } = TimeSpan.FromMinutes(10);

        // How many days readings are retained
        public int ReadingsRetentionDays { get; set; } = 30;
    }
}
