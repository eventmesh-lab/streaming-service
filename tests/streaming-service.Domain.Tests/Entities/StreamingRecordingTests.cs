using System;
using streaming_service.Domain.Entities;
using Xunit;

namespace streaming_service.Domain.Tests.Entities
{
    public class StreamingRecordingTests
    {
        [Fact]
        public void Constructor_ShouldInitializePropertiesCorrectly()
        {
            var sessionId = Guid.NewGuid();
            var storageUrl = "http://storage/rec.mp4";
            var size = 1024L;
            var duration = TimeSpan.FromMinutes(30);

            var before = DateTime.UtcNow;
            var sut = new StreamingRecording(sessionId, storageUrl, size, duration);
            var after = DateTime.UtcNow;

            Assert.NotEqual(Guid.Empty, sut.Id);
            Assert.Equal(sessionId, sut.SessionId);
            Assert.Equal(storageUrl, sut.StorageUrl);
            Assert.Equal(size, sut.FileSize);
            Assert.Equal(duration, sut.Duration);
            Assert.InRange(sut.CreatedAt, before.AddSeconds(-1), after.AddSeconds(1));
        }
    }
}
