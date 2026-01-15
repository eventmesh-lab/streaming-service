using System;

namespace streaming_service.Domain.Entities
{
    public class StreamingSession
    {
        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
        public DateTime ScheduledStartTime { get; private set; }
        public string Status { get; private set; } = "Scheduled";

        public StreamingSession() { }
    }
}
