using System;
using streaming_service.Domain.Entities;
using streaming_service.Domain.Enums;
using streaming_service.Domain.ValueObjects;
using Xunit;

namespace streaming_service.Domain.Tests.Entities
{
    public class StreamingSessionTests
    {
        [Fact]
        public void Constructor_ShouldInitializeSessionCorrectly()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddHours(1);
            int maxViewers = 100;

            // Act
            var session = new StreamingSession(eventId, startTime, maxViewers);

            // Assert
            Assert.NotEqual(Guid.Empty, session.Id);
            Assert.Equal(eventId, session.EventId);
            Assert.Equal(startTime, session.ScheduledStartTime);
            Assert.Equal(maxViewers, session.MaxViewers);
            Assert.Equal(StreamingStatus.Scheduled, session.Status);
            Assert.Null(session.ActualStartTime);
        }

        [Fact]
        public void StartSession_ShouldSetUrlAndStatusToLive()
        {
            // Arrange
            var session = new StreamingSession(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), 100);
            var url = new StreamingUrl("https://example.com/hls/manifest.m3u8");

            // Act
            session.StartSession(url);

            // Assert
            Assert.Equal(StreamingStatus.Live, session.Status);
            Assert.Equal(url, session.StreamUrl);
            Assert.NotNull(session.ActualStartTime);
        }

        [Fact]
        public void StartSession_WhenNotScheduled_ShouldThrowException()
        {
            // Arrange
            var session = new StreamingSession(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), 100);
            var url = new StreamingUrl("https://example.com/hls/manifest.m3u8");
            session.StartSession(url); // Now Live

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => session.StartSession(url));
            Assert.Contains("Scheduled", ex.Message);
        }

        [Fact]
        public void EndSession_ShouldSetStatusToEnded()
        {
            // Arrange
            var session = new StreamingSession(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), 100);
            var url = new StreamingUrl("https://example.com/hls/manifest.m3u8");
            session.StartSession(url);

            // Act
            session.EndSession();

            // Assert
            Assert.Equal(StreamingStatus.Ended, session.Status);
            Assert.NotNull(session.ActualEndTime);
        }

        [Fact]
        public void EndSession_WhenNotLive_ShouldThrowException()
        {
            // Arrange
            var session = new StreamingSession(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), 100);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => session.EndSession());
            Assert.Contains("live", ex.Message);
        }

        [Fact]
        public void MarkAsRecorded_ShouldSetStatusToRecorded()
        {
            // Arrange
            var session = new StreamingSession(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), 100);
            session.StartSession(new StreamingUrl("http://foo.bar"));
            session.EndSession();

            // Act
            session.MarkAsRecorded();

            // Assert
            Assert.Equal(StreamingStatus.Recorded, session.Status);
        }

        [Fact]
        public void MarkAsRecorded_WhenNotEnded_ShouldThrowException()
        {
             // Arrange
            var session = new StreamingSession(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), 100);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => session.MarkAsRecorded());
        }
    }
}
