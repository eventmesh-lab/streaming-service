using System;
using streaming_service.Domain.Enums;
using streaming_service.Domain.ValueObjects;

namespace streaming_service.Domain.Entities
{
    public class StreamingSession
    {
        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
        public StreamingUrl? StreamUrl { get; private set; }
        public StreamingStatus Status { get; private set; }
        public DateTime ScheduledStartTime { get; private set; }
        public DateTime? ActualStartTime { get; private set; }
        public DateTime? ActualEndTime { get; private set; }
        public int MaxViewers { get; private set; }

        private StreamingSession() { }

        public StreamingSession(Guid eventId, DateTime scheduledStartTime, int maxViewers)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            ScheduledStartTime = scheduledStartTime;
            MaxViewers = maxViewers;
            Status = StreamingStatus.Scheduled;
        }

        public void StartSession(StreamingUrl streamUrl)
        {
            if (Status != StreamingStatus.Scheduled)
                throw new InvalidOperationException("Session can only be started from Scheduled status.");

            StreamUrl = streamUrl;
            ActualStartTime = DateTime.UtcNow;
            Status = StreamingStatus.Live;
        }

        public void EndSession()
        {
            if (Status != StreamingStatus.Live)
                throw new InvalidOperationException("Only live sessions can be ended.");

            ActualEndTime = DateTime.UtcNow;
            Status = StreamingStatus.Ended;
        }

        public void MarkAsRecorded()
        {
            if (Status != StreamingStatus.Ended)
                throw new InvalidOperationException("Only ended sessions can be marked as recorded.");

            Status = StreamingStatus.Recorded;
        }
    }
}
