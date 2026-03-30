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
using Microsoft.EntityFrameworkCore;

using Domain.Models;

namespace Application.Services
{
    public class AccountService : IAccountService
    {
        protected readonly IUnitOfWork _uow;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly AppDbContext _dbContext;

        public AccountService(
            IUnitOfWork uow,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IJwtTokenGenerator jwtTokenGenerator,
            AppDbContext dbContext)
        {
            _uow = uow;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _dbContext = dbContext;
        }

        public async Task<Result<string>> LoginAsync(string login, string password)
        {
            var user = await _userManager.FindByNameAsync(login);
            if (user is null || !await _userManager.CheckPasswordAsync(user, password))
                return Result<string>.Failure("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);

            string token = _jwtTokenGenerator.GenerateToken(user, roles);

            return Result<string>.Success(token);
        }

        public async Task<Result> CreateAccountAsync(string requesterId, string userName, string password, IEnumerable<string>? roles)
        {
            var requester = await _userManager.FindByIdAsync(requesterId);
            if (requester is null) return Result.Failure("Requester not found");
            var requesterRoles = await _userManager.GetRolesAsync(requester);
            if (!requesterRoles.Contains("Admin")) return Result.Failure("Forbidden");

            var user = new IdentityUser { UserName = userName };
            var res = await _userManager.CreateAsync(user, password);
            if (!res.Succeeded) return Result.Failure(string.Join(';', res.Errors.Select(e => e.Description)));

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
                    return Result.Failure($"Roles do not exist: {string.Join(',', missing)}");
                }

                var sanitized = provided.Where(r => r != "Admin");
                if (sanitized.Any()) await _userManager.AddToRolesAsync(user, sanitized);
            }

            await LogAsync("User", "Create", requesterId, user.Id, $"Created user {user.UserName}", null, System.Text.Json.JsonSerializer.Serialize(new { user.UserName, roles = roles?.ToArray() ?? Array.Empty<string>() }));

            return Result.Success();
        }

        public async Task<Result> ChangePasswordAsync(string requesterId, string targetUserId, string? currentPassword, string newPassword)
        {
            var requester = await _userManager.FindByIdAsync(requesterId);
            if (requester is null) return Result.Failure("Requester not found");

            var target = await _userManager.FindByIdAsync(targetUserId);
            if (target is null) return Result.Failure("Target user not found");

            var requesterRoles = await _userManager.GetRolesAsync(requester);
            var isAdmin = requesterRoles.Contains("Admin");

            if (!isAdmin)
            {
                if (requesterId != targetUserId) return Result.Failure("Forbidden");
                if (string.IsNullOrEmpty(currentPassword)) return Result.Failure("Current password required");
                var ok = await _userManager.CheckPasswordAsync(target, currentPassword);
                if (!ok) return Result.Failure("Current password incorrect");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(target);
            var changeRes = await _userManager.ResetPasswordAsync(target, token, newPassword);
            if (!changeRes.Succeeded) return Result.Failure(string.Join(';', changeRes.Errors.Select(e => e.Description)));

            await LogAsync("User", "ChangePassword", requesterId, target.Id, $"Password changed for {target.UserName}", null, null);

            return Result.Success();
        }

        public async Task<Result> ChangeRolesAsync(string requesterId, string targetUserId, IEnumerable<string> roles)
        {
            var requester = await _userManager.FindByIdAsync(requesterId);
            if (requester is null) return Result.Failure("Requester not found");

            var requesterRoles = await _userManager.GetRolesAsync(requester);
            if (!requesterRoles.Contains("Admin")) return Result.Failure("Forbidden");

            var target = await _userManager.FindByIdAsync(targetUserId);
            if (target is null) return Result.Failure("Target user not found");

            var targetRoles = await _userManager.GetRolesAsync(target);
            if (targetRoles.Contains("Admin")) return Result.Failure("Cannot change roles of an Admin");

            var provided = roles.ToArray();
            var missing = new List<string>();
            foreach (var r in provided)
            {
                if (!await _roleManager.RoleExistsAsync(r)) missing.Add(r);
            }
            if (missing.Any()) return Result.Failure($"Roles do not exist: {string.Join(',', missing)}");

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

        public async Task<Result> DeleteUserAsync(string requesterId, string targetUserId)
        {
            var requester = await _userManager.FindByIdAsync(requesterId);
            if (requester is null) return Result.Failure("Requester not found");

            var requesterRoles = await _userManager.GetRolesAsync(requester);
            if (!requesterRoles.Contains("Admin")) return Result.Failure("Forbidden");

            if (requesterId == targetUserId) return Result.Failure("Admin cannot delete themselves");

            var target = await _userManager.FindByIdAsync(targetUserId);
            if (target is null) return Result.Failure("Target user not found");

            var delRes = await _userManager.DeleteAsync(target);
            if (!delRes.Succeeded) return Result.Failure(string.Join(';', delRes.Errors.Select(e => e.Description)));

            await LogAsync("User", "Delete", requesterId, target.Id, $"Deleted user {target.UserName}", null, null);

            return Result.Success();
        }

        public async Task<Result<(IEnumerable<UserDto> Items, int TotalCount)>> GetUsersAsync(int skip, int take, string? q = null, string? role = null)
        {
            if (skip < 0) return Result<(IEnumerable<UserDto> Items, int TotalCount)>.Failure("skip cannot be negative");
            if (take <= 0) return Result<(IEnumerable<UserDto> Items, int TotalCount)>.Failure("take must be positive");

            var query = _userManager.Users.AsNoTracking().AsQueryable();

            var search = q?.Trim();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.UserName!.Contains(search) ||
                    (u.Email != null && u.Email.Contains(search)));
            }

            var roleFilter = role?.Trim();
            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                var roleUserIds = from userRole in _dbContext.UserRoles
                                  join identityRole in _dbContext.Roles on userRole.RoleId equals identityRole.Id
                                  where identityRole.Name == roleFilter
                                  select userRole.UserId;

                query = query.Where(u => roleUserIds.Contains(u.Id));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.UserName)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();
            var rolePairs = await (
                from userRole in _dbContext.UserRoles
                join identityRole in _dbContext.Roles on userRole.RoleId equals identityRole.Id
                where userIds.Contains(userRole.UserId)
                select new { userRole.UserId, RoleName = identityRole.Name! }
            ).ToListAsync();

            var rolesByUser = rolePairs
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToArray());

            var list = new List<UserDto>();
            foreach (var u in users)
            {
                rolesByUser.TryGetValue(u.Id, out var roles);
                list.Add(new UserDto
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email,
                    Roles = roles ?? Array.Empty<string>()
                });
            }

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
