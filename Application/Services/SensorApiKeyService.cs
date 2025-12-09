using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Application.Interfaces;
using Application.Results.Base;

using Domain.Models;

using Infrastructure.Interfaces;
using Infrastructure.UOW;

using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class SensorApiKeyService : ServiceBase<SensorApiKey>, ISensorApiKeyService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher<SensorApiKey> _hasher;

        public SensorApiKeyService(IPasswordHasher<SensorApiKey> hasher, ISensorApiKeyRepository repository, IUnitOfWork uow) : base(repository, uow)
        {
            _hasher = hasher;
            _uow = uow;
        }

        public async Task<Result<int>> AuthenticationAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Result<int>.Failure("Key is empty");

            var all = await _uow.SensorApiKeys.GetAllAsync();
            var active = all.Where(x => x.IsActive).ToList();

            foreach (var entry in active)
            {
                var verification = _hasher.VerifyHashedPassword(entry, entry.ApiKeyHash, key);
                if (verification == PasswordVerificationResult.Success || verification == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    if (entry.SensorId.HasValue)
                        return Result<int>.Success(entry.SensorId.Value);

                    return Result<int>.Failure("ApiKey is not associated with a sensor");
                }
            }

            return Result<int>.Failure("Invalid api key");
        }

        public async Task<Result<string>> CreateNewApiKey(int sensorId)
        {
            // ensure sensor exists
            var sensor = await _uow.Sensors.GetAsync(sensorId);
            if (sensor is null)
                return Result<string>.Failure("Sensor not found");

            // deactivate existing active keys for this sensor
            var all = await _uow.SensorApiKeys.GetAllAsync();
            var activeForSensor = all.Where(k => k.SensorId == sensorId && k.IsActive).ToList();

            foreach (var k in activeForSensor)
            {
                k.IsActive = false;
                _uow.SensorApiKeys.Update(k);
            }

            // generate new plaintext key
            var plainBytes = new byte[32];
            RandomNumberGenerator.Fill(plainBytes);
            var plainKey = Convert.ToBase64String(plainBytes);

            var newKey = new SensorApiKey
            {
                SensorId = sensorId,
                IsActive = true
            };

            newKey.ApiKeyHash = _hasher.HashPassword(newKey, plainKey);

            await _uow.SensorApiKeys.AddAsync(newKey);

            await _uow.SaveChangesAsync();

            return Result<string>.Success(plainKey);
        }

        public async Task<bool> ValidateAsync(string key)
        {
            var res = await AuthenticationAsync(key);
            return res.IsSucceed;
        }
    }
}
