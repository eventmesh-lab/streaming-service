using System;
using System.Collections.Generic;
using System.Linq;
using streaming_service.Domain.ValueObjects;

namespace streaming_service.Domain.Entities
{
    public class StreamingAccess
    {
        private readonly List<DateTime> _reconnections = new();

        public Guid Id { get; private set; }
        public Guid SessionId { get; private set; }
        public Guid UserId { get; private set; }
        public Guid ReservationId { get; private set; }
        public AccessToken Token { get; private set; }
        public IReadOnlyCollection<DateTime> Reconnections => _reconnections.AsReadOnly();
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

            var now = DateTime.UtcNow;
            var oneHourAgo = now.AddHours(-1);
            
            // Check reconnection limit (Max 3 in 1 hour)
            int countInLastHour = _reconnections.Count(x => x > oneHourAgo);
            if (countInLastHour >= 3)
                throw new InvalidOperationException("Reconnection limit exceeded (Max 3 access per hour).");

            _reconnections.Add(now);
            AccessCount++;
            LastAccessAt = now;
        }

        public void RotateToken(AccessToken newToken)
        {
            if (Token.IsRefreshExpired)
                throw new InvalidOperationException("Refresh token has expired.");
            
            Token = newToken ?? throw new ArgumentNullException(nameof(newToken));
        }
    }
}
