using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.Results.Base;

using Domain.Models;

namespace Application.Interfaces
{
    public interface IReadOnlyService<T> where T : EntityBase
    {
        Task<Result<T>> Get(int id);
        Task<Result<IEnumerable<T>>> GetAll();
    }
}
