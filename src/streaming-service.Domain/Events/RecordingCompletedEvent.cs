using System;

namespace streaming_service.Domain.Events
{
    public record RecordingCompletedEvent(Guid RecordingId, Guid SessionId, string StorageUrl);
}
