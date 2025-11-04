using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Models;

using Infrastructure.Interfaces;

namespace Infrastructure.Repositories
{
 public class ReadingRepository : Repository<Reading>, IReadingRepository
 {
 public ReadingRepository(AppDbContext context) : base(context) { }
 }
}
