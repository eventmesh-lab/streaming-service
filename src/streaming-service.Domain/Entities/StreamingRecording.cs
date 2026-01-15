using System;

namespace streaming_service.Domain.Entities
{
    public class StreamingRecording
    {
        public Guid Id { get; private set; }
        public Guid SessionId { get; private set; }
        public string StorageUrl { get; private set; } = string.Empty;
        public long SizeBytes { get; private set; }
        public TimeSpan Duration { get; private set; }

        public StreamingRecording() { }
    }
}
