using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using streaming_service.Application.Commands;
using streaming_service.Application.Handlers;
using streaming_service.Domain.Entities;
using streaming_service.Domain.Ports;
using Xunit;

namespace streaming_service.Application.Tests.Handlers
{
    public class CreateSessionHandlerTests
    {
        private readonly Mock<IStreamingSessionRepository> _sessionRepositoryMock;
        private readonly Mock<IMessagePublisher> _messagePublisherMock;
        private readonly CreateSessionHandler _handler;

        public CreateSessionHandlerTests()
        {
            _sessionRepositoryMock = new Mock<IStreamingSessionRepository>();
            _messagePublisherMock = new Mock<IMessagePublisher>();
            _handler = new CreateSessionHandler(_sessionRepositoryMock.Object, _messagePublisherMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateSessionAndPublishEvent()
        {
            // Arrange
            var command = new CreateSessionCommand(
                Guid.NewGuid(), 
                DateTime.UtcNow.AddHours(2), 
                200
            );

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            
            // Verify Repository execution
            _sessionRepositoryMock.Verify(x => x.AddAsync(
                It.Is<StreamingSession>(s => 
                    s.EventId == command.EventId && 
                    s.MaxViewers == command.MaxViewers), 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            // Verify Publisher execution
            _messagePublisherMock.Verify(x => x.PublishAsync(
                It.IsAny<object>(),
                "streaming-exchange", 
                "session.created", 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }
    }
}
