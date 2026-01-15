using System;
using streaming_service.Domain.Entities;
using streaming_service.Domain.ValueObjects;
using Xunit;

namespace streaming_service.Domain.Tests.Entities
{
    public class StreamingAccessTests
    {
        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();
            var token = new AccessToken 
            { 
                Token = "valid_token", 
                RefreshToken = "refresh", 
                ExpiresAt = DateTime.UtcNow.AddHours(1), 
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var access = new StreamingAccess(sessionId, userId, reservationId, token);

            // Assert
            Assert.Equal(sessionId, access.SessionId);
            Assert.Equal(userId, access.UserId);
            Assert.Equal(reservationId, access.ReservationId);
            Assert.Equal(token, access.Token);
            Assert.Equal(0, access.AccessCount);
            Assert.Null(access.LastAccessAt);
            Assert.Empty(access.Reconnections);
        }

        [Fact]
        public void RecordAccess_ShouldIncrementCounter_WhenTokenValidAndLimitNotReached()
        {
            // Arrange
            var token = new AccessToken 
            { 
                Token = "valid_token", 
                RefreshToken = "refresh", 
                ExpiresAt = DateTime.UtcNow.AddHours(1), 
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(1)
            };
            var access = new StreamingAccess(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), token);

            // Act
            access.RecordAccess();

            // Assert
            Assert.Equal(1, access.AccessCount);
            Assert.Single(access.Reconnections);
            Assert.NotNull(access.LastAccessAt);
        }

        [Fact]
        public void RecordAccess_WhenTokenExpired_ShouldThrowException()
        {
            // Arrange
            // Create an expired token
            var token = new AccessToken 
            { 
                Token = "expired", 
                RefreshToken = "refresh", 
                ExpiresAt = DateTime.UtcNow.AddHours(-1), 
                RefreshTokenExpiresAt = DateTime.UtcNow.AddHours(1) 
            };
            var access = new StreamingAccess(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), token);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => access.RecordAccess());
            Assert.Contains("expired token", ex.Message);
        }

        [Fact]
        public void RecordAccess_WhenLimitExceeded_ShouldThrowException()
        {
            // Arrange
            var token = new AccessToken 
            { 
                Token = "valid", 
                RefreshToken = "refresh", 
                ExpiresAt = DateTime.UtcNow.AddHours(1), 
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(1) 
            };
            var access = new StreamingAccess(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), token);

            // Simulate 3 accesses
            access.RecordAccess();
            access.RecordAccess();
            access.RecordAccess();

            Assert.Equal(3, access.AccessCount);

            // Act & Assert 4th access
            var ex = Assert.Throws<InvalidOperationException>(() => access.RecordAccess());
            Assert.Contains("Reconnection limit exceeded", ex.Message);
        }

        [Fact]
        public void RotateToken_ShouldUpdateToken()
        {
            // Arrange
            var token = new AccessToken 
            { 
                Token = "start", 
                RefreshToken = "refresh", 
                ExpiresAt = DateTime.UtcNow.AddHours(1), 
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(1) 
            };
            var access = new StreamingAccess(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), token);
            var newToken = new AccessToken 
            { 
                Token = "new", 
                RefreshToken = "new_refresh", 
                ExpiresAt = DateTime.UtcNow.AddHours(2), 
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(2) 
            };

            // Act
            access.RotateToken(newToken);

            // Assert
            Assert.Equal(newToken, access.Token);
            Assert.Equal("new", access.Token.Token);
        }

        [Fact]
        public void RotateToken_WhenRefreshTokenExpired_ShouldThrowException()
        {
             // Arrange
            var token = new AccessToken 
            { 
                Token = "start", 
                RefreshToken = "refresh", 
                ExpiresAt = DateTime.UtcNow.AddHours(1), 
                RefreshTokenExpiresAt = DateTime.UtcNow.AddHours(-1) // Refresh expired
            }; 
            var access = new StreamingAccess(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), token);
            var newToken = new AccessToken 
            { 
                Token = "new", 
                RefreshToken = "r", 
                ExpiresAt = DateTime.UtcNow.AddHours(2), 
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(2) 
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => access.RotateToken(newToken));
        }
    }
}
