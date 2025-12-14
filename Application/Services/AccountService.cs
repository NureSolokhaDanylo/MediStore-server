using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.Interfaces;
using Application.Results.Base;
using Application.DTOs;

using Infrastructure.UOW;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class AccountService : IAccountService
    {
        protected readonly IUnitOfWork _uow;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AccountService(IUnitOfWork uow, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenGenerator jwtTokenGenerator)
        {
            _uow = uow;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
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
            // only admin can create
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
                    // rollback created user
                    await _userManager.DeleteAsync(user);
                    return Result.Failure($"Roles do not exist: {string.Join(',', missing)}");
                }

                // do not allow granting Admin role via this method (explicit)
                var sanitized = provided.Where(r => r != "Admin");
                if (sanitized.Any()) await _userManager.AddToRolesAsync(user, sanitized);
            }

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
                // user can change only their own password and must provide current password
                if (requesterId != targetUserId) return Result.Failure("Forbidden");
                if (string.IsNullOrEmpty(currentPassword)) return Result.Failure("Current password required");
                var ok = await _userManager.CheckPasswordAsync(target, currentPassword);
                if (!ok) return Result.Failure("Current password incorrect");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(target);
            var changeRes = await _userManager.ResetPasswordAsync(target, token, newPassword);
            if (!changeRes.Succeeded) return Result.Failure(string.Join(';', changeRes.Errors.Select(e => e.Description)));

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

            // prevent changing roles of Admins
            var targetRoles = await _userManager.GetRolesAsync(target);
            if (targetRoles.Contains("Admin")) return Result.Failure("Cannot change roles of an Admin");

            // validate requested roles exist
            var provided = roles.ToArray();
            var missing = new List<string>();
            foreach (var r in provided)
            {
                if (!await _roleManager.RoleExistsAsync(r)) missing.Add(r);
            }
            if (missing.Any()) return Result.Failure($"Roles do not exist: {string.Join(',', missing)}");

            // apply roles: remove all current except Admin, then add provided (but cannot grant Admin)
            var toRemove = targetRoles.Where(r => r != "Admin").ToArray();
            if (toRemove.Any()) await _userManager.RemoveFromRolesAsync(target, toRemove);

            var toAdd = provided.Where(r => r != "Admin").ToArray();
            if (toAdd.Any()) await _userManager.AddToRolesAsync(target, toAdd);

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

            return Result.Success();
        }

        public async Task<Result<IEnumerable<UserDto>>> GetUsersAsync(int skip, int take)
        {
            // get users sorted by username
            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            var list = new List<UserDto>();
            foreach (var u in users)
            {
                // call GetRolesAsync sequentially to avoid concurrent DbContext operations
                var roles = (await _userManager.GetRolesAsync(u)).ToArray();
                list.Add(new UserDto { Id = u.Id, UserName = u.UserName, Email = u.Email, Roles = roles });
            }

            return Result<IEnumerable<UserDto>>.Success(list);
        }
    }
}
