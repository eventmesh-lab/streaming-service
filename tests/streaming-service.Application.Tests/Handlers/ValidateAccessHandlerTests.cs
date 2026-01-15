using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using streaming_service.Application.Queries;
using streaming_service.Application.Handlers;
using streaming_service.Domain.Entities;
using streaming_service.Domain.Ports;
using streaming_service.Domain.ValueObjects;
using Xunit;

namespace streaming_service.Application.Tests.Handlers
{
    public class ValidateAccessHandlerTests
    {
        private readonly Mock<IStreamingAccessRepository> _accessRepoMock;
        private readonly Mock<IStreamingSessionRepository> _sessionRepoMock;
        private readonly Mock<IAuditLogger> _auditLoggerMock;
        private readonly ValidateAccessHandler _handler;

        public ValidateAccessHandlerTests()
        {
            _accessRepoMock = new Mock<IStreamingAccessRepository>();
            _sessionRepoMock = new Mock<IStreamingSessionRepository>();
            _auditLoggerMock = new Mock<IAuditLogger>();
            _handler = new ValidateAccessHandler(_accessRepoMock.Object, _sessionRepoMock.Object, _auditLoggerMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidToken_ShouldReturnStreamUrl()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var tokenStr = "valid_token_string";
            var token = new AccessToken 
            { 
                Token = tokenStr, 
                RefreshToken = "ref", 
                ExpiresAt = DateTime.UtcNow.AddHours(1), 
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(1)
            };
            
            var access = new StreamingAccess(sessionId, Guid.NewGuid(), Guid.NewGuid(), token);
            var session = new StreamingSession(Guid.NewGuid(), DateTime.UtcNow, 100);
            session.StartSession(new StreamingUrl("http://stream.url"));

            _accessRepoMock.Setup(x => x.GetByTokenAsync(tokenStr, It.IsAny<CancellationToken>()))
                .ReturnsAsync(access);
            
            _sessionRepoMock.Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(session);

            _accessRepoMock.Setup(x => x.GetAccessCountBySessionIdAsync(sessionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(50); // Under capacity

            var query = new ValidateAccessQuery(tokenStr);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal("http://stream.url", result.Value);
            
            // Verify Access Recorded
            _accessRepoMock.Verify(x => x.UpdateAsync(access, It.IsAny<CancellationToken>()), Times.Once);
            
            // Verify Audit
            _auditLoggerMock.Verify(x => x.LogAccessAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithUnknownToken_ShouldThrowException()
        {
             // Arrange
            _accessRepoMock.Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((StreamingAccess?)null);

            var query = new ValidateAccessQuery("unknown_token");

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WhenSessionNotLive_ShouldThrowException()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var token = new AccessToken 
            { 
                Token = "valid", 
                RefreshToken = "r", 
                ExpiresAt = DateTime.UtcNow.AddHours(1), 
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(1)
            };
            var access = new StreamingAccess(sessionId, Guid.NewGuid(), Guid.NewGuid(), token);
            
            var session = new StreamingSession(Guid.NewGuid(), DateTime.UtcNow, 100); 
            // Session is Scheduled by default, not Live

            _accessRepoMock.Setup(x => x.GetByTokenAsync("valid", It.IsAny<CancellationToken>())).ReturnsAsync(access);
            _sessionRepoMock.Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(new ValidateAccessQuery("valid"), CancellationToken.None));
            Assert.Contains("not available", ex.Message.ToLower());
        }
    }
}
