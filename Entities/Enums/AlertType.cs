using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum AlertType
    {
        ExpirationSoon = 1,
        Expired = 2,
        BatchConditionWarning = 3,
        ZoneConditionAlert = 4
    }
}
