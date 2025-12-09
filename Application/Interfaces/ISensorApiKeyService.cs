using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.Results.Base;

using Domain.Models;

namespace Application.Interfaces
{
    public interface ISensorApiKeyService : IService<SensorApiKey>
    {
        Task<Result<int>> AuthenticationAsync(string key);
        Task<Result<string>> CreateNewApiKey(int sensorId);
        Task<bool> ValidateAsync(string key);
    }

}
