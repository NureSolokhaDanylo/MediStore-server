using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/ping")]
    [Produces("text/plain")]
    public class PingController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult Ping()
        {
            // write to console for simple health check
            System.Console.WriteLine("ping");
            return Ok("pong");
        }
    }
}
