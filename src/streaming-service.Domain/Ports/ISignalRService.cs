using System.Threading;
using System.Threading.Tasks;

namespace streaming_service.Domain.Ports
{
    public interface ISignalRService
    {
        Task NotifyUserAsync(string userId, string message, object? data, CancellationToken cancellationToken);
        Task BroadcastAsync(string message, object? data, CancellationToken cancellationToken);
    }
}
