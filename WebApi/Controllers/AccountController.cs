using System.Data;
using System.Security.Claims;

using Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs.AccountDTOs;
using WebApi.DTOs;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/account")]
    public class AccountController : MyController
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var res = await _accountService.LoginAsync(dto.Login, dto.Password);
            if (!res.IsSucceed) return ApiErrorResult(res);
            return Ok(new { token = res.Value });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var id = userId;
            var name = login;
            if (string.IsNullOrEmpty(id)) return UnauthorizedErrorResult();
            return Ok(new { Id = id, Login = name, Roles = roles });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
        {
            var requester = userId;
            if (string.IsNullOrEmpty(requester)) return UnauthorizedErrorResult();

            var res = await _accountService.CreateAccountAsync(requester, dto.UserName, dto.Password, dto.Roles);
            if (!res.IsSucceed) return ApiErrorResult(res);
            return NoContent();
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var requester = userId;
            if (string.IsNullOrEmpty(requester)) return UnauthorizedErrorResult();

            var target = string.IsNullOrEmpty(dto.TargetUserId) ? requester : dto.TargetUserId;
            var res = await _accountService.ChangePasswordAsync(requester, target, dto.CurrentPassword, dto.NewPassword);
            if (!res.IsSucceed) return ApiErrorResult(res);
            return NoContent();
        }

        [HttpPost("roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRoles([FromBody] ChangeRolesDto dto)
        {
            var requester = userId;
            if (string.IsNullOrEmpty(requester)) return UnauthorizedErrorResult();

            var res = await _accountService.ChangeRolesAsync(requester, dto.TargetUserId, dto.Roles);
            if (!res.IsSucceed) return ApiErrorResult(res);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var requester = userId;
            if (string.IsNullOrEmpty(requester)) return UnauthorizedErrorResult();

            var res = await _accountService.DeleteUserAsync(requester, id);
            if (!res.IsSucceed) return ApiErrorResult(res);
            return NoContent();
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResultDto<UserDto>>> GetUsers(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            [FromQuery] string? q = null,
            [FromQuery] string? role = null)
        {
            if (skip < 0) return ValidationErrorResult<PagedResultDto<UserDto>>("skip cannot be negative", ApiErrorCodes.Account.InvalidPaging);
            if (take <= 0) return ValidationErrorResult<PagedResultDto<UserDto>>("take must be positive", ApiErrorCodes.Account.InvalidPaging);

            var res = await _accountService.GetUsersAsync(skip, take, q, role);
            if (!res.IsSucceed) return ApiErrorResult<PagedResultDto<UserDto>>(res);

            var (items, totalCount) = res.Value!;
            var appUsers = items ?? Enumerable.Empty<Application.DTOs.UserDto>();
            var dtoList = appUsers.Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Roles = u.Roles
            });

            return Ok(new PagedResultDto<UserDto>
            {
                Items = dtoList,
                TotalCount = totalCount,
                Skip = skip,
                Take = take
            });
        }
    }
}
