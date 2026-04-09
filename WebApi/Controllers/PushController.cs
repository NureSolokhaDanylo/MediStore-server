using Application.DTOs.UserDevice;
using Application.Interfaces;
using Application.Results.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApi.DTOs;

namespace WebApi.Controllers
{
    [Route("api/v1/push/devices")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class PushController : MyController
    {
        private readonly IUserDeviceService _service;

        public PushController(IUserDeviceService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RegisterDevice([FromBody] CreateUserDeviceDto dto)
        {
            var result = await _service.RegisterDeviceAsync(dto);

            if (!result.IsSucceed)
            {
                return ApiErrorResult(result);
            }

            return NoContent();
        }
    }
}
