using System;
using MediatR;
using streaming_service.Domain.ValueObjects;

namespace streaming_service.Application.Queries
{
    public record ValidateAccessQuery(string Token) : IRequest<StreamingUrl>;
}
