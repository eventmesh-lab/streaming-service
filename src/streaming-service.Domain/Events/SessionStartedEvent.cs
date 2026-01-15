using System;

namespace streaming_service.Domain.Events
{
    public record SessionStartedEvent(Guid SessionId, Guid EventId, DateTime StartedAt);
}
