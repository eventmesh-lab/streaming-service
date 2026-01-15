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
    public class RefreshTokenHandlerTests
    {
        private readonly Mock<IStreamingAccessRepository> _accessRepo;
        private readonly Mock<ITokenGenerator> _tokenGen;
        private readonly RefreshTokenHandler _handler;

        public RefreshTokenHandlerTests()
        {
            _accessRepo = new Mock<IStreamingAccessRepository>();
            _tokenGen = new Mock<ITokenGenerator>();
            _handler = new RefreshTokenHandler(_accessRepo.Object, _tokenGen.Object);
        }

        [Fact]
        public async Task Handle_WithValidTokens_ShouldRotateToken()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var oldToken = AccessToken.Create("old_token", "refresh_123", userId, sessionId);
            
            var access = new StreamingAccess(sessionId, userId, Guid.NewGuid(), oldToken);
            
            _accessRepo.Setup(x => x.GetByTokenAsync("old_token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(access);

            var newToken = AccessToken.Create("new_token", "new_refresh", userId, sessionId);
            _tokenGen.Setup(x => x.RefreshToken(oldToken))
                .Returns(newToken);

            var cmd = new RefreshTokenCommand("old_token", "refresh_123");

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            Assert.Equal("new_token", result.Token);
            Assert.Equal("new_token", access.Token.Token); 
            _accessRepo.Verify(x => x.UpdateAsync(access, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidRefreshToken_ShouldThrowException()
        {
            // Arrange
            var oldToken = AccessToken.Create("old", "refresh_correct", Guid.NewGuid(), Guid.NewGuid());
            var access = new StreamingAccess(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), oldToken);

            _accessRepo.Setup(x => x.GetByTokenAsync("old", It.IsAny<CancellationToken>()))
                .ReturnsAsync(access);

            var cmd = new RefreshTokenCommand("old", "refresh_WRONG");

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(cmd, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WhenRefreshExpired_ShouldThrowException()
        {
            // Arrange
            var oldToken = new AccessToken 
            { 
                Token = "old", 
                RefreshToken = "refresh", 
                RefreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(-10) 
            };
            var access = new StreamingAccess(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), oldToken);

            _accessRepo.Setup(x => x.GetByTokenAsync("old", It.IsAny<CancellationToken>()))
                .ReturnsAsync(access);

            var cmd = new RefreshTokenCommand("old", "refresh");

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(cmd, CancellationToken.None));
        }
    }
}
