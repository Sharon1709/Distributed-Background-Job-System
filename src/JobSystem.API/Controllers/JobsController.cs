using JobSystem.Application.Jobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly CreateJobHandler _handler;

        public JobsController(CreateJobHandler handler)
        {
            _handler = handler;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateJobRequest request)
        {
            var jobId = await _handler.HandleAsync(request.Payload, CancellationToken.None);
            return Ok(new { JobId = jobId });
        }
    }

    public record CreateJobRequest(string Payload);
}
