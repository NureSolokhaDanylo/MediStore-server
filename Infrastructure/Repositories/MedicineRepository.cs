using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Models;

using Infrastructure.Interfaces;

namespace Infrastructure.Repositories
{
 public class MedicineRepository : Repository<Medicine>, IMedicineRepository
 {
 public MedicineRepository(AppDbContext context) : base(context) { }
 }
}
