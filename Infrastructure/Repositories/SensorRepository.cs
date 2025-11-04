using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Models;

using Infrastructure.Interfaces;

namespace Infrastructure.Repositories
{
 public class SensorRepository : Repository<Sensor>, ISensorRepository
 {
 public SensorRepository(AppDbContext context) : base(context) { }
 }
}
