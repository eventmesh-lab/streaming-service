using System;
using System.Threading;
using System.Threading.Tasks;
using streaming_service.Domain.Entities;

namespace streaming_service.Domain.Ports
{
    public interface IStreamingAccessRepository
    {
        Task AddAsync(StreamingAccess access, CancellationToken cancellationToken = default);
        Task UpdateAsync(StreamingAccess access, CancellationToken cancellationToken = default);
        Task<StreamingAccess?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<StreamingAccess?> GetByUserIdAndSessionIdAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
        Task<int> GetAccessCountBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    }
}
