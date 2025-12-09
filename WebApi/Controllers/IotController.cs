using Application.Attributes;

using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("iot/data")]
    [RequireSensorApiKey]
    public class IotController : MyController
    {
    }
}
