using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using streaming_service.Application.Queries;
using streaming_service.Domain.Ports;
using streaming_service.Domain.Enums;

namespace streaming_service.Application.Handlers
{
    public class GetStreamAccessQueryHandler : IRequestHandler<GetStreamAccessQuery, StreamAccessResult>
    {
        private readonly IStreamingSessionRepository _sessionRepository;
        private readonly IStreamingAccessRepository _accessRepository;

        public GetStreamAccessQueryHandler(
            IStreamingSessionRepository sessionRepository,
            IStreamingAccessRepository accessRepository)
        {
            _sessionRepository = sessionRepository;
            _accessRepository = accessRepository;
        }

        public async Task<StreamAccessResult> Handle(GetStreamAccessQuery request, CancellationToken cancellationToken)
        {
            // 1. Get active session for event
            var session = await _sessionRepository.GetByEventIdAsync(request.EventId, cancellationToken);
            
            if (session == null)
                return StreamAccessResult.Failure("No active streaming session found for this event");

            // 2. Validate timing: user can access 10 minutes before scheduled start
            var now = DateTime.UtcNow;
            var allowedAccessTime = session.ScheduledStartTime.AddMinutes(-10);
            
            if (now < allowedAccessTime)
                return StreamAccessResult.Failure($"Access not yet available. Stream opens at {allowedAccessTime:HH:mm} UTC");

            // 3. Check if session is live or scheduled
            if (session.Status != StreamingStatus.Live && session.Status != StreamingStatus.Scheduled)
                return StreamAccessResult.Failure("Stream is not available");

            // 4. Verify user has access record (from reservation confirmation)
            var access = await _accessRepository.GetByUserAndSessionAsync(request.UserId, session.Id, cancellationToken);
            
            if (access == null)
                return StreamAccessResult.Failure("No valid access token found. Please check your reservation");

            // 5. Check capacity
            var currentViewers = await _accessRepository.GetAccessCountBySessionIdAsync(session.Id, cancellationToken);
            
            if (currentViewers >= session.MaxViewers)
                return StreamAccessResult.Failure("Stream capacity reached. You are in the waiting queue");

            // 6. Generate/return stream URL
            var streamUrl = session.StreamUrl?.Value ?? GenerateSimulatedStreamUrl(session.Id);

            // 7. Return success with stream details
            var expiresAt = session.ScheduledStartTime.AddHours(4); // Stream access expires 4 hours after start
            
            return StreamAccessResult.Success(streamUrl, session.Id, expiresAt);
        }

        private string GenerateSimulatedStreamUrl(Guid sessionId)
        {
            // For MVP: return a simulated stream URL
            // In production, this would be a signed URL from your streaming provider
            return $"https://storage.example.com/streams/{sessionId}/stream.m3u8";
        }
    }
}
