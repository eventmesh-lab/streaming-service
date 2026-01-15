using System;

namespace streaming_service.Application.Commands
{
    // Note: Depends on MediatR
    public record CreateSessionCommand(Guid EventId, DateTime ScheduledStartTime);
}
