using System;
using System.Collections.Generic;
using streaming_service.Domain.ValueObjects;

namespace streaming_service.Domain.Ports
{
    public interface ITokenGenerator
    {
        AccessToken GenerateToken(Guid userId, Guid sessionId, Guid reservationId, Dictionary<string, object>? claims = null);
        AccessToken RefreshToken(AccessToken expiredToken);
    }
}
