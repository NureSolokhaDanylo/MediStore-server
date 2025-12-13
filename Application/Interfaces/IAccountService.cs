using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Results.Base;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IAccountService
    {
        Task<Result<string>> LoginAsync(string login, string password);
        Task<Result> CreateAccountAsync(string requesterId, string userName, string password, IEnumerable<string>? roles);
        Task<Result> ChangePasswordAsync(string requesterId, string targetUserId, string? currentPassword, string newPassword);
        Task<Result> ChangeRolesAsync(string requesterId, string targetUserId, IEnumerable<string> roles);
        Task<Result> DeleteUserAsync(string requesterId, string targetUserId);
        Task<Result<IEnumerable<UserDto>>> GetUsersAsync(int skip, int take);
    }
}
