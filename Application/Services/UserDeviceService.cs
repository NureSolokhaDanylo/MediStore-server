using Application.DTOs.UserDevice;
using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.UOW;
using System;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserDeviceService : IUserDeviceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        public UserDeviceService(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        public async Task<Result> RegisterDeviceAsync(string userId, CreateUserDeviceDto dto)
        {
            if (userId != dto.UserId)
            {
                return Result.Failure(Errors.Forbidden(ErrorCodes.Push.UserMismatch, "User ID mismatch"));
            }

            var existingDevice = await _unitOfWork.UserDevices.GetByUserIdAsync(dto.UserId);

            if (existingDevice != null)
            {
                existingDevice.FcmToken = dto.FcmToken;
                existingDevice.LastSeenAt = DateTime.UtcNow;
                _unitOfWork.UserDevices.Update(existingDevice);
            }
            else
            {
                var newDevice = new UserDevice
                {
                    UserId = dto.UserId,
                    FcmToken = dto.FcmToken,
                    CreatedAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow
                };
                await _unitOfWork.UserDevices.AddAsync(newDevice);
            }

            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
    }
}
