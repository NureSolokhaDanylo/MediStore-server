using Domain.Models;
using Application.Results.Base;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IMedicineService : ICrudService<Medicine>
{
}
