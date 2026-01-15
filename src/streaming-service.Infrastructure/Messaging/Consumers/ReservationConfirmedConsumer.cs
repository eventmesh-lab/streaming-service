using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using streaming_service.Application.Commands;

namespace streaming_service.Infrastructure.Messaging.Consumers
{
    // Integrated with Módulo de Reservas
    public class ReservationConfirmedConsumer
    {
        private readonly IMediator _mediator;

        public ReservationConfirmedConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task ConsumeAsync(ReservationConfirmedEvent message, CancellationToken ct = default)
        {
            // When a reservation is confirmed, automatically generate a token
            await _mediator.Send(new GenerateAccessTokenCommand(
                message.SessionId, 
                message.UserId, 
                message.ReservationId), ct);
            
            Console.WriteLine($"Token generated for Reservation {message.ReservationId}");
        }
    }

    public record ReservationConfirmedEvent(Guid ReservationId, Guid UserId, Guid SessionId, Guid EventId);
}
