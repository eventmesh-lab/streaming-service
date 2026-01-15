using System;
using System.Threading;
using System.Threading.Tasks;
using streaming_service.Domain.Entities;
using streaming_service.Domain.Ports;

namespace streaming_service.Infrastructure.Services
{
    public class StubStreamingRecordingService : IStreamingRecordingService
    {
        public Task StartRecordingAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            // Placeholder: In real implementation, this would call Firebase/MediaServer to START
            // For stub, we just pretend we did it.
            return Task.CompletedTask;
        }

        public Task<StreamingRecording> StopRecordingAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            // Placeholder: When stopping, we pretend we got the file details back
            var recording = new StreamingRecording(
                sessionId, 
                $"https://storage.googleapis.com/simulated-bucket/{sessionId}.mp4",
                1024 * 1024 * 500, // 500 MB mock
                TimeSpan.FromHours(2) // 2 hours mock
            );
            
            return Task.FromResult(recording);
        }
    }
}
