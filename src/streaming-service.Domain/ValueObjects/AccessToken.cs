using System;

namespace streaming_service.Domain.ValueObjects
{
    public record AccessToken
    {
        public string Token { get; init; } = string.Empty;
        public DateTime ExpiresAt { get; init; }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }
}
