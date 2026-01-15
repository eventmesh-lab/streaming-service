using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using streaming_service.Domain.Entities;
using streaming_service.Domain.Ports;
using streaming_service.Application.Commands;

namespace streaming_service.Application.Handlers
{
    public class ProcessRecordingHandler : IRequestHandler<ProcessRecordingCommand>
    {
        private readonly IStreamingSessionRepository _sessionRepository;
        private readonly IMessagePublisher _publisher;
        private readonly ISignalRService _signalRService;
        private readonly IImageProcessor _imageProcessor;

        public ProcessRecordingHandler(
            IStreamingSessionRepository sessionRepository,
            IMessagePublisher publisher,
            ISignalRService signalRService,
            IImageProcessor imageProcessor)
        {
            _sessionRepository = sessionRepository;
            _publisher = publisher;
            _signalRService = signalRService;
            _imageProcessor = imageProcessor;
        }

        public async Task Handle(ProcessRecordingCommand request, CancellationToken cancellationToken)
        {
            var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (session == null) throw new InvalidOperationException("Session not found.");

            // Feature D: Generate thumbnail (ImageSharp logic abstraction)
            var thumbnailUrl = await _imageProcessor.GenerateThumbnailAsync(request.StorageUrl, cancellationToken);

            // Create recording entity
            var recording = new StreamingRecording(session.Id, request.StorageUrl, request.FileSize, request.Duration);
            
            // Mark session as recorded (Domain logic)
            session.MarkAsRecorded();
            await _sessionRepository.UpdateAsync(session, cancellationToken);

            // Publish RecordingReady event to RabbitMQ (CU-ST-04)
            await _publisher.PublishAsync(new
            {
                RecordingId = recording.Id,
                SessionId = session.Id,
                StorageUrl = recording.StorageUrl,
                ThumbnailUrl = thumbnailUrl,
                Duration = recording.Duration,
                EventType = "RecordingReady"
            }, exchange: "streaming-exchange", routingKey: "recording.ready", cancellationToken: cancellationToken);

            // Notify organizers via SignalR (CU-ST-04)
            await _signalRService.BroadcastAsync(
                "RecordingReady", 
                new { RecordingId = recording.Id, SessionId = session.Id, Thumbnail = thumbnailUrl }, 
                cancellationToken);
        }
    }
}
