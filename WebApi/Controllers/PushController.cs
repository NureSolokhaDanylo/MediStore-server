using Application.DTOs.UserDevice;
using Application.Interfaces;
using Application.Results.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/push/devices")]
    [ApiController]
    public class PushController : MyController
    {
        private readonly IUserDeviceService _service;

        public PushController(IUserDeviceService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RegisterDevice([FromBody] CreateUserDeviceDto dto)
        {
            // Validate that the user ID in the body matches the authenticated user
            if (userId != dto.UserId)
            {
                return Forbid();
            }

            var result = await _service.RegisterDeviceAsync(userId!, dto);

            if (!result.IsSucceed)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok();
        }
    }
}
