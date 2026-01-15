using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;

namespace streaming_service.Api.Hubs
{
    public class StreamingHub : Hub
    {
        private static readonly ConcurrentDictionary<string, int> _sessionViewers = new();
        private static readonly ConcurrentDictionary<string, int> _sessionCapacity = new();

        public async Task JoinSession(string sessionId, int capacity)
        {
            _sessionCapacity.TryAdd(sessionId, capacity);
            
            int currentViewers = _sessionViewers.GetOrAdd(sessionId, 0);
            
            if (currentViewers >= capacity)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Capacity full. You are in the waiting queue.");
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Queue_{sessionId}");
                return;
            }

            _sessionViewers.AddOrUpdate(sessionId, 1, (key, val) => val + 1);
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            await Clients.Group(sessionId).SendAsync("ViewerCountUpdated", _sessionViewers[sessionId]);
            await Clients.Caller.SendAsync("AccessGranted", "Welcome to the stream!");
        }

        public async Task LeaveSession(string sessionId)
        {
            _sessionViewers.AddOrUpdate(sessionId, 0, (key, val) => Math.Max(0, val - 1));
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
            await Clients.Group(sessionId).SendAsync("ViewerCountUpdated", _sessionViewers[sessionId]);
        }

        public async Task SendChatMessage(string sessionId, string message)
        {
            var username = Context.User?.Identity?.Name ?? "Anonymous";
            
            await Clients.Group(sessionId).SendAsync("ReceiveChatMessage", new
            {
                username,
                text = message,
                timestamp = DateTime.UtcNow
            });
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var sessionId in _sessionViewers.Keys)
            {
                _sessionViewers.AddOrUpdate(sessionId, 0, (key, val) => Math.Max(0, val - 1));
                await Clients.Group(sessionId).SendAsync("ViewerCountUpdated", _sessionViewers[sessionId]);
                await Clients.Group($"Queue_{sessionId}").SendAsync("SpaceAvailable", "A spot has opened up!");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendSignal(string user, string signal)
        {
            await Clients.All.SendAsync("ReceiveSignal", user, signal);
        }
    }
}
