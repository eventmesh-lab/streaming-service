using System;
using MediatR;

namespace streaming_service.Application.Commands
{
    public record ProcessRecordingCommand(Guid SessionId, string StorageUrl, long FileSize, TimeSpan Duration) : IRequest;
}
