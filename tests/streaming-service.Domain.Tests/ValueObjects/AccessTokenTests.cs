using System;
using streaming_service.Domain.ValueObjects;
using Xunit;

namespace streaming_service.Domain.Tests.ValueObjects
{
    public class AccessTokenTests
    {
        [Fact]
        public void Create_ShouldInitializePropertiesCorrectly()
        {
            var tokenStr = "access_token";
            var refreshStr = "refresh_token";
            var userId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var expiry = 60;
            var days = 7;

            // Start time around now
            var before = DateTime.UtcNow;
            var token = AccessToken.Create(tokenStr, refreshStr, userId, sessionId, expiry, days);
            var after = DateTime.UtcNow;

            Assert.Equal(tokenStr, token.Token);
            Assert.Equal(refreshStr, token.RefreshToken);
            Assert.Equal(userId, token.UserId);
            Assert.Equal(sessionId, token.SessionId);
            
            // Allow small delta for time
            Assert.InRange(token.ExpiresAt, before.AddMinutes(expiry).AddSeconds(-1), after.AddMinutes(expiry).AddSeconds(1));
            Assert.InRange(token.RefreshTokenExpiresAt, before.AddDays(days).AddSeconds(-1), after.AddDays(days).AddSeconds(1));
        }

        [Fact]
        public void IsExpired_ShouldReturnTrueIfPastExpiry()
        {
            var token = new AccessToken 
            { 
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1) 
            };
            Assert.True(token.IsExpired);
        }

        [Fact]
        public void IsRefreshExpired_ShouldReturnTrueIfPastExpiry()
        {
            var token = new AccessToken 
            { 
                RefreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(-1)
            };
            Assert.True(token.IsRefreshExpired);
        }

        [Fact]
        public void IsExpired_ShouldReturnFalseIfFuture()
        {
            var token = new AccessToken 
            { 
                ExpiresAt = DateTime.UtcNow.AddMinutes(1) 
            };
            Assert.False(token.IsExpired);
        }
    }
}
