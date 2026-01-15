using System;
using System.Threading;
using System.Threading.Tasks;
using streaming_service.Domain.Ports;

namespace streaming_service.Infrastructure.Services
{
    public class StubImageProcessor : IImageProcessor
    {
        public Task<string> GenerateThumbnailAsync(string videoUrl, CancellationToken cancellationToken = default)
        {
            // Placeholder: Return a dummy image URL
            return Task.FromResult($"https://via.placeholder.com/640x360.png?text=Thumbnail+{Guid.NewGuid()}");
        }
    }
}
