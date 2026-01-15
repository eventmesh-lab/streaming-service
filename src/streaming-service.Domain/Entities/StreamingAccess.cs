using System;

namespace streaming_service.Domain.Entities
{
    public class StreamingAccess
    {
        public Guid Id { get; private set; }
        public Guid SessionId { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime AccessGrantedAt { get; private set; }

        public StreamingAccess() { }
    }
}
