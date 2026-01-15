using System.Threading;
using System.Threading.Tasks;
using MediatR;
using streaming_service.Domain.Entities;
using streaming_service.Domain.Ports;
using streaming_service.Domain.ValueObjects;
using streaming_service.Application.Commands;

namespace streaming_service.Application.Handlers
{
    public class GenerateAccessTokenHandler : IRequestHandler<GenerateAccessTokenCommand, AccessToken>
    {
        private readonly IStreamingAccessRepository _accessRepository;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly ISignalRService _signalRService;

        public GenerateAccessTokenHandler(
            IStreamingAccessRepository accessRepository, 
            ITokenGenerator tokenGenerator,
            ISignalRService signalRService)
        {
            _accessRepository = accessRepository;
            _tokenGenerator = tokenGenerator;
            _signalRService = signalRService;
        }

        public async Task<AccessToken> Handle(GenerateAccessTokenCommand request, CancellationToken cancellationToken)
        {
            // Generate the token using the infrastructure service via port
            var token = _tokenGenerator.GenerateToken(request.UserId, request.SessionId, request.ReservationId, null);

            var access = new StreamingAccess(request.SessionId, request.UserId, request.ReservationId, token);
            
            await _accessRepository.AddAsync(access, cancellationToken);

            // Notify user via SignalR (CU-ST-02)
            await _signalRService.NotifyUserAsync(
                request.UserId.ToString(), 
                "Your streaming access token is ready.", 
                new { Token = token.Token, SessionId = request.SessionId }, 
                cancellationToken);

            return token;
        }
    }
}
