using System;
using streaming_service.Domain.ValueObjects;

namespace streaming_service.Domain.Entities
{
    public class StreamingAccess
    {
        public Guid Id { get; private set; }
        public Guid SessionId { get; private set; }
        public Guid UserId { get; private set; }
        public Guid ReservationId { get; private set; }
        public AccessToken Token { get; private set; }
        public int AccessCount { get; private set; }
        public DateTime? LastAccessAt { get; private set; }

        private StreamingAccess() { }

        public StreamingAccess(Guid sessionId, Guid userId, Guid reservationId, AccessToken token)
        {
            Id = Guid.NewGuid();
            SessionId = sessionId;
            UserId = userId;
            ReservationId = reservationId;
            Token = token ?? throw new ArgumentNullException(nameof(token));
            AccessCount = 0;
        }

        public void RecordAccess()
        {
            if (Token.IsExpired)
                throw new InvalidOperationException("Cannot record access with an expired token.");

            AccessCount++;
            LastAccessAt = DateTime.UtcNow;
        }
    }
}
