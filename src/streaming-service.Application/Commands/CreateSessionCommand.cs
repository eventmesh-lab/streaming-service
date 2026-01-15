using System;
using MediatR;

namespace streaming_service.Application.Commands
{
    public record CreateSessionCommand(Guid EventId, DateTime ScheduledStartTime, int MaxViewers) : IRequest<Guid>;
}
