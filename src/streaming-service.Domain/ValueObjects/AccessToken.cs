using System;

namespace streaming_service.Domain.ValueObjects
{
    public record AccessToken
    {
        public string Token { get; init; } = string.Empty;
        public DateTime ExpiresAt { get; init; }
        public Guid UserId { get; init; }
        public Guid SessionId { get; init; }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        public static AccessToken Create(string token, Guid userId, Guid sessionId, int expirationMinutes = 60)
        {
            return new AccessToken
            {
                Token = token,
                UserId = userId,
                SessionId = sessionId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };
        }
    }
}
