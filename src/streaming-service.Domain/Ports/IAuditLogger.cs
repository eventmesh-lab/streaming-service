using System.Threading;
using System.Threading.Tasks;

namespace streaming_service.Domain.Ports
{
    public interface IAuditLogger
    {
        Task LogAccessAsync(object accessLog, CancellationToken cancellationToken = default);
    }
}
