using System;
using MediatR;
using streaming_service.Domain.ValueObjects;

namespace streaming_service.Application.Commands
{
    public record GenerateAccessTokenCommand(Guid SessionId, Guid UserId, Guid ReservationId) : IRequest<AccessToken>;
}
