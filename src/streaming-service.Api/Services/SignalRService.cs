using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using streaming_service.Api.Hubs;
using streaming_service.Domain.Ports;

namespace streaming_service.Api.Services
{
    public class SignalRService : ISignalRService
    {
        private readonly IHubContext<StreamingHub> _hubContext;

        public SignalRService(IHubContext<StreamingHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyUserAsync(string userId, string message, object data, CancellationToken cancellationToken = default)
        {
            // In a real app, map UserId -> ConnectionId. 
            // For now, assume using UserId as the group or identifier if configured.
            // Or simplification: Broadcast to Client(userId) if UserId provider is set, OR to a Group named by UserId.
            await _hubContext.Clients.User(userId).SendAsync("Notification", new { Message = message, Data = data }, cancellationToken);
        }

        public async Task BroadcastAsync(string eventName, object data, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.All.SendAsync(eventName, data, cancellationToken);
        }
    }
}
