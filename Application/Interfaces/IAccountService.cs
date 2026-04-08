using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Results.Base;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IAccountService
    {
        Task<Result<string>> LoginAsync(string login, string password);
        Task<Result> CreateAccountAsync(string userName, string password, IEnumerable<string>? roles);
        Task<Result> ChangePasswordAsync(string? targetUserId, string? currentPassword, string newPassword);
        Task<Result> ChangeRolesAsync(string targetUserId, IEnumerable<string> roles);
        Task<Result> DeleteUserAsync(string targetUserId);
        Task<Result<(IEnumerable<UserDto> Items, int TotalCount)>> GetUsersAsync(int skip, int take, string? q = null, string? role = null);
    }
}
