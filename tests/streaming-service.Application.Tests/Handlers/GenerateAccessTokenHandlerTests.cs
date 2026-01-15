using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using streaming_service.Application.Commands;
using streaming_service.Application.Handlers;
using streaming_service.Domain.Entities;
using streaming_service.Domain.Ports;
using streaming_service.Domain.ValueObjects;
using Xunit;

namespace streaming_service.Application.Tests.Handlers
{
    public class GenerateAccessTokenHandlerTests
    {
        private readonly Mock<IStreamingAccessRepository> _accessRepo;
        private readonly Mock<ITokenGenerator> _tokenGen;
        private readonly Mock<ISignalRService> _signalR;
        private readonly GenerateAccessTokenHandler _handler;

        public GenerateAccessTokenHandlerTests()
        {
            _accessRepo = new Mock<IStreamingAccessRepository>();
            _tokenGen = new Mock<ITokenGenerator>();
            _signalR = new Mock<ISignalRService>();
            _handler = new GenerateAccessTokenHandler(_accessRepo.Object, _tokenGen.Object, _signalR.Object);
        }

        [Fact]
        public async Task Handle_ShouldGenerateTokenAndNotifyUser()
        {
            // Arrange
            var cmd = new GenerateAccessTokenCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            
            var generatedToken = AccessToken.Create("token", "refresh", cmd.UserId, cmd.SessionId);
            
            _tokenGen.Setup(x => x.GenerateToken(cmd.UserId, cmd.SessionId, cmd.ReservationId, It.IsAny<System.Collections.Generic.Dictionary<string, object>?>()))
                .Returns(generatedToken);

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            Assert.Equal(generatedToken, result);

            // Verify Persistence
            _accessRepo.Verify(x => x.AddAsync(It.Is<StreamingAccess>(a => 
                a.UserId == cmd.UserId && 
                a.SessionId == cmd.SessionId), It.IsAny<CancellationToken>()), Times.Once);

            // Verify Notification
            _signalR.Verify(x => x.NotifyUserAsync(
                cmd.UserId.ToString(), 
                It.Is<string>(s => s.Contains("ready")), 
                It.IsAny<object?>(), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
