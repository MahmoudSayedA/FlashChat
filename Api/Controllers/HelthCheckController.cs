using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/")]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet("health")]
        public IActionResult Health() => Ok("Healthy");

        [HttpGet("/")]
        public IActionResult Test() => Ok("Api is running.");

    }
}
