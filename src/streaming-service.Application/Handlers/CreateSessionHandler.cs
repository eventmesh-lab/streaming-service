using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using streaming_service.Domain.Entities;
using streaming_service.Domain.Ports;
using streaming_service.Application.Commands;

namespace streaming_service.Application.Handlers
{
    public class CreateSessionHandler : IRequestHandler<CreateSessionCommand, Guid>
    {
        private readonly IStreamingSessionRepository _repository;
        private readonly IMessagePublisher _publisher;

        public CreateSessionHandler(IStreamingSessionRepository repository, IMessagePublisher publisher)
        {
            _repository = repository;
            _publisher = publisher;
        }

        public async Task<Guid> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
        {
            var session = new StreamingSession(request.EventId, request.ScheduledStartTime, request.MaxViewers);
            
            await _repository.AddAsync(session, cancellationToken);
            
            // System publishes event to RabbitMQ
            await _publisher.PublishAsync(new
            {
                SessionId = session.Id,
                EventId = session.EventId,
                ScheduledStartTime = session.ScheduledStartTime,
                MaxViewers = session.MaxViewers,
                EventType = "StreamingSessionCreated"
            }, exchange: "streaming-exchange", routingKey: "session.created", cancellationToken: cancellationToken);

            return session.Id;
        }
    }
}
