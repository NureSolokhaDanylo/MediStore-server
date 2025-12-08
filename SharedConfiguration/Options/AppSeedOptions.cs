using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedConfiguration.Options
{
    public class AppSeedOptions
    {
        public string AdminLogin { get; set; } = null!;
        public string AdminPassword { get; set; } = null!;
    }
}
