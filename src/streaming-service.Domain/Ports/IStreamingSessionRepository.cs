using System;
using System.Threading;
using System.Threading.Tasks;
using streaming_service.Domain.Entities;

namespace streaming_service.Domain.Ports
{
    public interface IStreamingSessionRepository
    {
        Task AddAsync(StreamingSession session, CancellationToken cancellationToken = default);
        Task UpdateAsync(StreamingSession session, CancellationToken cancellationToken = default);
        Task<StreamingSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<StreamingSession?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    }
}
