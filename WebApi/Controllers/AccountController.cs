using System.Data;
using System.Security.Claims;

using Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs.AccountDTOs;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/account")]
    public class AccountController : MyController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AccountController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        // Returns current user id, username and roles (reads from DB via UserManager)
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            return Ok(new
            {
                UserId = userId,
                Username = login,
                Roles = roles
            });
        }

        // Alternative: read id/roles directly from the ClaimsPrincipal (no DB hit)
        [HttpGet("claims")]
        [Authorize]
        public IActionResult Claims()
        {
            var idFromClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rolesFromClaims = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

            return Ok(new { IdFromClaim = idFromClaim, RolesFromClaims = rolesFromClaims });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Login);
            if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);

            string token = _jwtTokenGenerator.GenerateToken(user, roles);

            return Ok(new { token });
        }

        [HttpGet("ping")]
        [Authorize]
        public IActionResult Ping()
        {
            return Ok();
        }

        //создать аккаунт (только админ может)
        //изменить пароль (каждый может, но так же админ может для кого угодно)
        //изменить роли (админ может выдать другим роли, но не админе)
        //удалить пользователя (админ может удалить любого, кроме себя)
    }
}
