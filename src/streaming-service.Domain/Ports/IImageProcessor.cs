using System.Threading;
using System.Threading.Tasks;

namespace streaming_service.Domain.Ports
{
    public interface IImageProcessor
    {
        Task<string> GenerateThumbnailAsync(string videoUrl, CancellationToken cancellationToken = default);
    }
}
