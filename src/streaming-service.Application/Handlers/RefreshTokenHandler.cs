using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using streaming_service.Domain.Ports;
using streaming_service.Domain.ValueObjects;
using streaming_service.Application.Commands;

namespace streaming_service.Application.Handlers
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AccessToken>
    {
        private readonly IStreamingAccessRepository _accessRepository;
        private readonly ITokenGenerator _tokenGenerator;

        public RefreshTokenHandler(IStreamingAccessRepository accessRepository, ITokenGenerator tokenGenerator)
        {
            _accessRepository = accessRepository;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<AccessToken> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var access = await _accessRepository.GetByTokenAsync(request.ExpiredToken, cancellationToken);
            if (access == null) throw new UnauthorizedAccessException("Invalid access record.");

            if (access.Token.RefreshToken != request.RefreshToken)
                throw new UnauthorizedAccessException("Invalid refresh token.");

            if (access.Token.IsRefreshExpired)
                throw new UnauthorizedAccessException("Refresh token has expired.");

            // Generate new token using port logic
            var newToken = _tokenGenerator.RefreshToken(access.Token);

            // Update domain entity
            access.RotateToken(newToken);
            await _accessRepository.UpdateAsync(access, cancellationToken);

            return newToken;
        }
    }
}
