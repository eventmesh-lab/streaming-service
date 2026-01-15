using System;

namespace streaming_service.Domain.ValueObjects
{
    public record AccessToken
    {
        public string Token { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
        public DateTime ExpiresAt { get; init; }
        public DateTime RefreshTokenExpiresAt { get; init; }
        public Guid UserId { get; init; }
        public Guid SessionId { get; init; }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsRefreshExpired => DateTime.UtcNow > RefreshTokenExpiresAt;

        public static AccessToken Create(string token, string refreshToken, Guid userId, Guid sessionId, int expirationMinutes = 60, int refreshExpirationDays = 7)
        {
            var now = DateTime.UtcNow;
            return new AccessToken
            {
                Token = token,
                RefreshToken = refreshToken,
                UserId = userId,
                SessionId = sessionId,
                ExpiresAt = now.AddMinutes(expirationMinutes),
                RefreshTokenExpiresAt = now.AddDays(refreshExpirationDays)
            };
        }
    }
}
