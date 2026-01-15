using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using streaming_service.Domain.Ports;
using streaming_service.Domain.ValueObjects;
using streaming_service.Application.Queries;

namespace streaming_service.Application.Handlers
{
    public class ValidateAccessHandler : IRequestHandler<ValidateAccessQuery, StreamingUrl>
    {
        private readonly IStreamingAccessRepository _accessRepository;
        private readonly IStreamingSessionRepository _sessionRepository;
        private readonly IAuditLogger _auditLogger;

        public ValidateAccessHandler(
            IStreamingAccessRepository accessRepository,
            IStreamingSessionRepository sessionRepository,
            IAuditLogger auditLogger)
        {
            _accessRepository = accessRepository;
            _sessionRepository = sessionRepository;
            _auditLogger = auditLogger;
        }

        public async Task<StreamingUrl> Handle(ValidateAccessQuery request, CancellationToken cancellationToken)
        {
            var access = await _accessRepository.GetByTokenAsync(request.Token, cancellationToken);
            if (access == null) throw new UnauthorizedAccessException("Invalid token.");

            if (access.Token.IsExpired) throw new UnauthorizedAccessException("Token has expired.");

            var session = await _sessionRepository.GetByIdAsync(access.SessionId, cancellationToken);
            if (session == null) throw new InvalidOperationException("Session not found.");

            // Check capacity
            var currentViewers = await _accessRepository.GetAccessCountBySessionIdAsync(session.Id, cancellationToken);
            if (currentViewers >= session.MaxViewers)
                throw new InvalidOperationException("Session capacity exceeded.");

            // Register access in audit log (CU-ST-03)
            await _auditLogger.LogAccessAsync(new
            {
                UserId = access.UserId,
                SessionId = session.Id,
                Timestamp = DateTime.UtcNow,
                Action = "StreamAccessValidated"
            }, cancellationToken);

            // Record access in domain entity
            access.RecordAccess();
            await _accessRepository.UpdateAsync(access, cancellationToken);

            return session.StreamUrl ?? throw new InvalidOperationException("Stream URL not available yet.");
        }
    }
}
