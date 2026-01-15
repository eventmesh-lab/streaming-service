using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using streaming_service.Application.Commands;
using streaming_service.Application.Queries;

namespace streaming_service.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StreamingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StreamingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("session")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionCommand command, CancellationToken ct)
        {
            var id = await _mediator.Send(command, ct);
            return Ok(new { SessionId = id });
        }

        [HttpPost("token")]
        public async Task<IActionResult> GenerateToken([FromBody] GenerateAccessTokenCommand command, CancellationToken ct)
        {
            var token = await _mediator.Send(command, ct);
            return Ok(token);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken ct)
        {
            var token = await _mediator.Send(command, ct);
            return Ok(token);
        }

        [HttpGet("validate")]
        public async Task<IActionResult> ValidateAccess([FromQuery] string token, CancellationToken ct)
        {
            var url = await _mediator.Send(new ValidateAccessQuery(token), ct);
            return Ok(new { StreamUrl = url.Value, IsEncrypted = url.IsEncrypted });
        }

        // Feature C: Simulated HLS/DASH Streaming pattern
        [HttpGet("stream/{eventId}/{token}")]
        public IActionResult GetStreamPattern(Guid eventId, string token)
        {
            // This endpoint simulates the entry point for a player
            return Ok(new
            {
                Type = "HLS",
                ManifestUrl = $"/api/streaming/mock/{eventId}/playlist.m3u8?token={token}",
                LicenseUrl = "/api/streaming/mock/license",
                Metadata = new
                {
                    Title = "Evento en Vivo",
                    Resolution = "1080p",
                    Framerate = 60,
                    IsLive = true
                }
            });
        }

        [HttpGet("mock/{eventId}/playlist.m3u8")]
        public IActionResult GetMockPlaylist(Guid eventId, [FromQuery] string token)
        {
            // Simulated HLS playlist
            string content = "#EXTM3U\n#EXT-X-VERSION:3\n#EXT-X-TARGETDURATION:10\n#EXT-X-MEDIA-SEQUENCE:0\n" +
                             $"#EXTINF:10.0,\n/api/streaming/mock/{eventId}/chunk_0.ts\n" +
                             $"#EXTINF:10.0,\n/api/streaming/mock/{eventId}/chunk_1.ts\n" +
                             "#EXT-X-ENDLIST";
            
            return Content(content, "application/vnd.apple.mpegurl");
        }
    }
}
