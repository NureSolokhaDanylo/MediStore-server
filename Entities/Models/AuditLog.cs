using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class AuditLog : EntityBase
    {
        public DateTime OccurredAt { get; set; }

        public string EntityType { get; set; } = null!; 
        public int EntityId { get; set; }

        public string Action { get; set; } = null!;     

        public string? UserId { get; set; }             

        public string? Summary { get; set; }            

        public string? OldValues { get; set; }          
        public string? NewValues { get; set; }          
    }

}
