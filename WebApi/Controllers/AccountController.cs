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
            if (!res.IsSucceed) return Unauthorized(res.ErrorMessage);
            return Ok(new { token = res.Value });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var id = userId;
            var name = login;
            if (string.IsNullOrEmpty(id)) return Unauthorized();
            return Ok(new { Id = id, Login = name, Roles = roles });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
        {
            var requester = userId;
            if (string.IsNullOrEmpty(requester)) return Unauthorized();

            var res = await _accountService.CreateAccountAsync(requester, dto.UserName, dto.Password, dto.Roles);
            if (!res.IsSucceed) return BadRequest(res.ErrorMessage);
            return NoContent();
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var requester = userId;
            if (string.IsNullOrEmpty(requester)) return Unauthorized();

            var target = string.IsNullOrEmpty(dto.TargetUserId) ? requester : dto.TargetUserId;
            var res = await _accountService.ChangePasswordAsync(requester, target, dto.CurrentPassword, dto.NewPassword);
            if (!res.IsSucceed) return BadRequest(res.ErrorMessage);
            return NoContent();
        }

        [HttpPost("roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRoles([FromBody] ChangeRolesDto dto)
        {
            var requester = userId;
            if (string.IsNullOrEmpty(requester)) return Unauthorized();

            var res = await _accountService.ChangeRolesAsync(requester, dto.TargetUserId, dto.Roles);
            if (!res.IsSucceed) return BadRequest(res.ErrorMessage);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var requester = userId;
            if (string.IsNullOrEmpty(requester)) return Unauthorized();

            var res = await _accountService.DeleteUserAsync(requester, id);
            if (!res.IsSucceed) return BadRequest(res.ErrorMessage);
            return NoContent();
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers([FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            var res = await _accountService.GetUsersAsync(skip, take);
            if (!res.IsSucceed) return BadRequest(res.ErrorMessage);

            var appUsers = res.Value ?? Enumerable.Empty<Application.DTOs.UserDto>();
            var dtoList = appUsers.Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Roles = u.Roles
            });

            return Ok(dtoList);
        }
    }
}
