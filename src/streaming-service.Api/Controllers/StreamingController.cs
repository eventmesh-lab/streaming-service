using Microsoft.AspNetCore.Mvc;

namespace streaming_service.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StreamingController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Streaming Service API");
        }
    }
}
