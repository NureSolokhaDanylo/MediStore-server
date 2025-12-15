using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/ping")]
    public class PingController : ControllerBase
    {
        [HttpGet]
        public IActionResult Ping()
        {
            // write to console for simple health check
            System.Console.WriteLine("ping");
            return Ok("pong");
        }
    }
}
