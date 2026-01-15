using System.Threading;
using System.Threading.Tasks;

namespace streaming_service.Domain.Ports
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message, string exchange = "", string routingKey = "", CancellationToken cancellationToken = default);
    }
}
