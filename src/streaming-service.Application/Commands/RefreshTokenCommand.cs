using System;
using MediatR;
using streaming_service.Domain.ValueObjects;

namespace streaming_service.Application.Commands
{
    public record RefreshTokenCommand(string ExpiredToken, string RefreshToken) : IRequest<AccessToken>;
}
