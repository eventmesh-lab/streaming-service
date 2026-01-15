using System;
using System.Threading;
using System.Threading.Tasks;
using streaming_service.Domain.Entities;

namespace streaming_service.Domain.Ports
{
    public interface IStreamingRecordingService
    {
        Task StartRecordingAsync(Guid sessionId, CancellationToken cancellationToken = default);
        Task<StreamingRecording> StopRecordingAsync(Guid sessionId, CancellationToken cancellationToken = default);
    }
}
