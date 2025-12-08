using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedConfiguration.Options
{
    public class AppIdentityOptions
    {
        public int PasswordRequiredLength { get; set; } = 8;
        public bool PasswordRequireNonAlphanumeric { get; set; } = false;
        public bool PasswordRequireUppercase { get; set; } = false;
        public bool PasswordRequireLowercase { get; set; } = false;
        public bool PasswordRequireDigit { get; set; } = false;
        public bool UserRequireUniqueEmail { get; set; } = false;
    }
}
