using System.Linq;
using streaming_service.Domain.ValueObjects;
using Xunit;

namespace streaming_service.Domain.Tests.ValueObjects
{
    public class StreamingQualityTests
    {
        [Fact]
        public void PredefinedQualities_ShouldHaveCorrectValues()
        {
            Assert.Equal("480p", StreamingQuality.P480.Resolution);
            Assert.Equal(1500, StreamingQuality.P480.BitrateKbps);

            Assert.Equal("720p", StreamingQuality.P720.Resolution);
            Assert.Equal(3000, StreamingQuality.P720.BitrateKbps);

            Assert.Equal("1080p", StreamingQuality.P1080.Resolution);
            Assert.Equal(6000, StreamingQuality.P1080.BitrateKbps);
        }

        [Fact]
        public void All_ShouldContainAllQualities()
        {
            var all = StreamingQuality.All.ToList();
            Assert.Equal(3, all.Count);
            Assert.Contains(StreamingQuality.P480, all);
            Assert.Contains(StreamingQuality.P720, all);
            Assert.Contains(StreamingQuality.P1080, all);
        }
    }
}
