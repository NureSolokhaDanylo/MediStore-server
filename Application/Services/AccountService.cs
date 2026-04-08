using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.Interfaces;
using Application.Results.Base;
using Application.DTOs;

using Infrastructure;
using Infrastructure.UOW;

using Microsoft.AspNetCore.Identity;

using Domain.Models;
using Infrastructure.Interfaces;

namespace Application.Services
{
    public class AccountService : IAccountService
    {
        protected readonly IUnitOfWork _uow;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IAccessChecker _accessChecker;

        public AccountService(
            IUnitOfWork uow,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IJwtTokenGenerator jwtTokenGenerator,
            IUserRepository userRepository,
            ICurrentUser currentUser,
            IAccessChecker accessChecker)
        {
            _uow = uow;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _userRepository = userRepository;
            _currentUser = currentUser;
            _accessChecker = accessChecker;
        }

        public async Task<Result<string>> LoginAsync(string login, string password)
        {
            var user = await _userManager.FindByNameAsync(login);
            if (user is null || !await _userManager.CheckPasswordAsync(user, password))
                return Result<string>.Failure(AuthErrors.InvalidCredentials());

            var roles = await _userManager.GetRolesAsync(user);

            string token = _jwtTokenGenerator.GenerateToken(user, roles);

            return Result<string>.Success(token);
        }

        public async Task<Result> CreateAccountAsync(string userName, string password, IEnumerable<string>? roles)
        {
            var auth = _accessChecker.EnsureCurrentUserInRole("Admin");
            if (!auth.IsSucceed) return auth;

            var requesterId = _currentUser.UserId!;
            var requester = await _userManager.FindByIdAsync(requesterId);
            if (requester is null) return Result.Failure(Errors.NotFound(ErrorCodes.Account.RequesterNotFound, "Requester not found", "requesterId", requesterId));

            var user = new IdentityUser { UserName = userName };
            var res = await _userManager.CreateAsync(user, password);
            if (!res.Succeeded) return Result.Failure(Errors.Validation(ErrorCodes.Account.CreateFailed, string.Join(';', res.Errors.Select(e => e.Description))));

            if (roles != null)
            {
                var provided = roles.ToArray();
                var missing = new List<string>();
                foreach (var r in provided)
                {
                    if (!await _roleManager.RoleExistsAsync(r)) missing.Add(r);
                }
                if (missing.Any())
                {
                    await _userManager.DeleteAsync(user);
                    return Result.Failure(Errors.Validation(
                        ErrorCodes.Account.RolesDoNotExist,
                        $"Roles do not exist: {string.Join(',', missing)}",
                        details: new Dictionary<string, object?> { ["roles"] = missing.ToArray() }));
                }

                var sanitized = provided.Where(r => r != "Admin");
                if (sanitized.Any()) await _userManager.AddToRolesAsync(user, sanitized);
            }

            await LogAsync("User", "Create", requesterId, user.Id, $"Created user {user.UserName}", null, System.Text.Json.JsonSerializer.Serialize(new { user.UserName, roles = roles?.ToArray() ?? Array.Empty<string>() }));

            return Result.Success();
        }

        public async Task<Result> ChangePasswordAsync(string? targetUserId, string? currentPassword, string newPassword)
        {
            var auth = _accessChecker.EnsureAuthenticated();
            if (!auth.IsSucceed) return auth;

            var requesterId = _currentUser.UserId!;
            var requester = await _userManager.FindByIdAsync(requesterId);
            if (requester is null) return Result.Failure(Errors.NotFound(ErrorCodes.Account.RequesterNotFound, "Requester not found", "requesterId", requesterId));

            var effectiveTargetUserId = string.IsNullOrWhiteSpace(targetUserId) ? requesterId : targetUserId;
            var target = await _userManager.FindByIdAsync(effectiveTargetUserId);
            if (target is null) return Result.Failure(Errors.NotFound(ErrorCodes.Account.TargetUserNotFound, "Target user not found", "targetUserId", effectiveTargetUserId));

            var isAdmin = _currentUser.IsInRole("Admin");

            if (!isAdmin)
            {
                var access = _accessChecker.EnsureCurrentUserMatches(effectiveTargetUserId, AuthErrors.Forbidden());
                if (!access.IsSucceed) return access;
                if (string.IsNullOrEmpty(currentPassword)) return Result.Failure(AuthErrors.CurrentPasswordRequired());
                var ok = await _userManager.CheckPasswordAsync(target, currentPassword);
                if (!ok) return Result.Failure(AuthErrors.CurrentPasswordIncorrect());
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(target);
            var changeRes = await _userManager.ResetPasswordAsync(target, token, newPassword);
            if (!changeRes.Succeeded) return Result.Failure(Errors.Validation(ErrorCodes.Account.ChangePasswordFailed, string.Join(';', changeRes.Errors.Select(e => e.Description))));

            await LogAsync("User", "ChangePassword", requesterId, target.Id, $"Password changed for {target.UserName}", null, null);

            return Result.Success();
        }

        public async Task<Result> ChangeRolesAsync(string targetUserId, IEnumerable<string> roles)
        {
            var auth = _accessChecker.EnsureCurrentUserInRole("Admin");
            if (!auth.IsSucceed) return auth;

            var requesterId = _currentUser.UserId!;
            var requester = await _userManager.FindByIdAsync(requesterId);
            if (requester is null) return Result.Failure(Errors.NotFound(ErrorCodes.Account.RequesterNotFound, "Requester not found", "requesterId", requesterId));

            var target = await _userManager.FindByIdAsync(targetUserId);
            if (target is null) return Result.Failure(Errors.NotFound(ErrorCodes.Account.TargetUserNotFound, "Target user not found", "targetUserId", targetUserId));

            var targetRoles = await _userManager.GetRolesAsync(target);
            if (targetRoles.Contains("Admin")) return Result.Failure(Errors.Conflict(ErrorCodes.Account.CannotChangeAdminRoles, "Cannot change roles of an Admin"));

            var provided = roles.ToArray();
            var missing = new List<string>();
            foreach (var r in provided)
            {
                if (!await _roleManager.RoleExistsAsync(r)) missing.Add(r);
            }
            if (missing.Any()) return Result.Failure(Errors.Validation(
                ErrorCodes.Account.RolesDoNotExist,
                $"Roles do not exist: {string.Join(',', missing)}",
                details: new Dictionary<string, object?> { ["roles"] = missing.ToArray() }));

            var toRemove = targetRoles.Where(r => r != "Admin").ToArray();
            if (toRemove.Any()) await _userManager.RemoveFromRolesAsync(target, toRemove);

            var toAdd = provided.Where(r => r != "Admin").ToArray();
            if (toAdd.Any()) await _userManager.AddToRolesAsync(target, toAdd);

            await LogAsync(
                "User",
                "ChangeRoles",
                requesterId,
                target.Id,
                $"Roles changed for {target.UserName}",
                System.Text.Json.JsonSerializer.Serialize(new { roles = targetRoles }),
                System.Text.Json.JsonSerializer.Serialize(new { roles = toAdd }));

            return Result.Success();
        }

        public async Task<Result> DeleteUserAsync(string targetUserId)
        {
            var auth = _accessChecker.EnsureCurrentUserInRole("Admin");
            if (!auth.IsSucceed) return auth;

            var requesterId = _currentUser.UserId!;
            var requester = await _userManager.FindByIdAsync(requesterId);
            if (requester is null) return Result.Failure(Errors.NotFound(ErrorCodes.Account.RequesterNotFound, "Requester not found", "requesterId", requesterId));

            if (requesterId == targetUserId) return Result.Failure(Errors.Conflict(ErrorCodes.Account.CannotDeleteSelf, "Admin cannot delete themselves"));

            var target = await _userManager.FindByIdAsync(targetUserId);
            if (target is null) return Result.Failure(Errors.NotFound(ErrorCodes.Account.TargetUserNotFound, "Target user not found", "targetUserId", targetUserId));

            var delRes = await _userManager.DeleteAsync(target);
            if (!delRes.Succeeded) return Result.Failure(Errors.Validation(ErrorCodes.Account.DeleteFailed, string.Join(';', delRes.Errors.Select(e => e.Description))));

            await LogAsync("User", "Delete", requesterId, target.Id, $"Deleted user {target.UserName}", null, null);

            return Result.Success();
        }

        public async Task<Result<(IEnumerable<UserDto> Items, int TotalCount)>> GetUsersAsync(int skip, int take, string? q = null, string? role = null)
        {
            var access = _accessChecker.EnsureCurrentUserInRole("Admin");
            if (!access.IsSucceed) return Result<(IEnumerable<UserDto> Items, int TotalCount)>.Failure(access.Error!);

            if (skip < 0) return Result<(IEnumerable<UserDto> Items, int TotalCount)>.Failure(PagingErrors.InvalidSkip(ErrorCodes.Account.InvalidPaging));
            if (take <= 0) return Result<(IEnumerable<UserDto> Items, int TotalCount)>.Failure(PagingErrors.InvalidTake(ErrorCodes.Account.InvalidPaging));

            var (items, totalCount) = await _userRepository.GetUsersAsync(skip, take, q, role);
            var list = items.Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Roles = u.Roles
            }).ToList();

            return Result<(IEnumerable<UserDto> Items, int TotalCount)>.Success((list, totalCount));
        }

        private async Task LogAsync(string entityType, string action, string? userId, string entityId, string? summary, string? oldValues, string? newValues)
        {
            var log = new AuditLog
            {
                OccurredAt = DateTime.UtcNow,
                EntityType = entityType,
                EntityId = 0, // IdentityUser uses string ids; store real id in summary
                Action = action,
                UserId = userId,
                Summary = summary + $" (UserId: {entityId})",
                OldValues = oldValues,
                NewValues = newValues
            };

            await _uow.AuditLogs.AddAsync(log);
            await _uow.SaveChangesAsync();
        }
    }
}
