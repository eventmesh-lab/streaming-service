using System;
using MediatR;
using streaming_service.Domain.ValueObjects;

namespace streaming_service.Application.Queries
{
    public record GetStreamAccessQuery(Guid EventId, Guid UserId, string AccessToken) : IRequest<StreamAccessResult>;

    public class StreamAccessResult
    {
        public bool IsSuccess { get; private set; }
        public string? StreamUrl { get; private set; }
        public Guid? SessionId { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public string? ErrorMessage { get; private set; }

        private StreamAccessResult() { }

        public static StreamAccessResult Success(string streamUrl, Guid sessionId, DateTime expiresAt)
        {
            return new StreamAccessResult
            {
                IsSuccess = true,
                StreamUrl = streamUrl,
                SessionId = sessionId,
                ExpiresAt = expiresAt
            };
        }

        public static StreamAccessResult Failure(string errorMessage)
        {
            return new StreamAccessResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
