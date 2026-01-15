using System;
using System.Threading;
using System.Threading.Tasks;
using streaming_service.Domain.Enums;
using Moq;
using streaming_service.Application.Commands;
using streaming_service.Application.Handlers;
using streaming_service.Domain.Entities;
using streaming_service.Domain.Ports;
using streaming_service.Domain.ValueObjects;
using Xunit;

namespace streaming_service.Application.Tests.Handlers
{
    public class ProcessRecordingHandlerTests
    {
        private readonly Mock<IStreamingSessionRepository> _sessionRepo;
        private readonly Mock<IMessagePublisher> _publisher;
        private readonly Mock<ISignalRService> _signalRService;
        private readonly Mock<IImageProcessor> _imageProcessor;
        private readonly ProcessRecordingHandler _handler;

        public ProcessRecordingHandlerTests()
        {
            _sessionRepo = new Mock<IStreamingSessionRepository>();
            _publisher = new Mock<IMessagePublisher>();
            _signalRService = new Mock<ISignalRService>();
            _imageProcessor = new Mock<IImageProcessor>();
            _handler = new ProcessRecordingHandler(
                _sessionRepo.Object, 
                _publisher.Object, 
                _signalRService.Object, 
                _imageProcessor.Object);
        }

        [Fact]
        public async Task Handle_ShouldProcessRecordingAndMarkSession()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var session = new StreamingSession(eventId, DateTime.UtcNow, 100);
            session.StartSession(StreamingUrl.Create("http://live"));
            session.EndSession(); 

            _sessionRepo.Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(session);

            _imageProcessor.Setup(x => x.GenerateThumbnailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("http://thumb.jpg");

            var command = new ProcessRecordingCommand(sessionId, "http://rec.mp4", 1024, TimeSpan.FromHours(1));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(StreamingStatus.Recorded, session.Status);
            
            // Verify Updates
            _sessionRepo.Verify(x => x.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
            
            // Verify Publisher
            _publisher.Verify(x => x.PublishAsync(
                It.IsAny<object>(), 
                "streaming-exchange", 
                "recording.ready", 
                It.IsAny<CancellationToken>()), Times.Once);

            // Verify SignalR
            _signalRService.Verify(x => x.BroadcastAsync(
                "RecordingReady", 
                It.IsAny<object>(), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenSessionNotFound_ShouldThrowException()
        {
            _sessionRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((StreamingSession?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(new ProcessRecordingCommand(Guid.NewGuid(), "url", 0, TimeSpan.Zero), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WhenSessionNotEnded_ShouldThrowException()
        {
            var session = new StreamingSession(Guid.NewGuid(), DateTime.UtcNow, 100);
            // Status is Scheduled

            _sessionRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(session);

            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(new ProcessRecordingCommand(session.Id, "url", 0, TimeSpan.Zero), CancellationToken.None));
        }
    }
}
