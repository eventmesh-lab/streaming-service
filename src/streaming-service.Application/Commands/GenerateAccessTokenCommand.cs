using System;

namespace streaming_service.Application.Commands
{
    public record GenerateAccessTokenCommand(Guid SessionId, Guid UserId);
}
