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
    public class SensorApiKeyService : ISensorApiKeyService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher<SensorApiKey> _hasher;

        public SensorApiKeyService(IPasswordHasher<SensorApiKey> hasher, IUnitOfWork uow)
        {
            _hasher = hasher;
            _uow = uow;
        }

        public async Task<Result<int>> AuthenticationAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Result<int>.Failure(new ErrorInfo
                {
                    Code = "sensor_api_key.empty_key",
                    Message = "Key is empty",
                    Type = ErrorType.Unauthorized
                });

            var all = await _uow.SensorApiKeys.GetAllAsync();
            var active = all.Where(x => x.IsActive).ToList();

            foreach (var entry in active)
            {
                var verification = _hasher.VerifyHashedPassword(entry, entry.ApiKeyHash, key);
                if (verification == PasswordVerificationResult.Success || verification == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    if (entry.SensorId.HasValue)
                        return Result<int>.Success(entry.SensorId.Value);

                    return Result<int>.Failure(new ErrorInfo
                    {
                        Code = "sensor_api_key.not_bound_to_sensor",
                        Message = "ApiKey is not associated with a sensor",
                        Type = ErrorType.Unauthorized
                    });
                }
            }

            return Result<int>.Failure(new ErrorInfo
            {
                Code = "sensor_api_key.invalid_key",
                Message = "Invalid api key",
                Type = ErrorType.Unauthorized
            });
        }

        public async Task<Result<string>> CreateNewApiKey(int sensorId)
        {
            // ensure sensor exists
            var sensor = await _uow.Sensors.GetAsync(sensorId);
            if (sensor is null)
                return Result<string>.Failure(new ErrorInfo
                {
                    Code = "sensor_api_key.sensor_not_found",
                    Message = "Sensor not found",
                    Type = ErrorType.NotFound,
                    Details = new Dictionary<string, object?> { ["sensorId"] = sensorId }
                });

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
    }
}
