using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace streaming_service.Api.Hubs
{
    public class StreamingHub : Hub
    {
        public async Task SendSignal(string user, string signal)
        {
            await Clients.All.SendAsync("ReceiveSignal", user, signal);
        }
    }
}
