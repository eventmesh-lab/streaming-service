using System;

namespace streaming_service.Domain.Entities
{
    public class StreamingRecording
    {
        public Guid Id { get; private set; }
        public Guid SessionId { get; private set; }
        public string StorageUrl { get; private set; } = string.Empty;
        public long FileSize { get; private set; }
        public TimeSpan Duration { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private StreamingRecording() { }

        public StreamingRecording(Guid sessionId, string storageUrl, long fileSize, TimeSpan duration)
        {
            Id = Guid.NewGuid();
            SessionId = sessionId;
            StorageUrl = storageUrl;
            FileSize = fileSize;
            Duration = duration;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
