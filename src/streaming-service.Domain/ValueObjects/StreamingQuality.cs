using System;
using System.Collections.Generic;

namespace streaming_service.Domain.ValueObjects
{
    public record StreamingQuality
    {
        public string Resolution { get; init; }
        public int BitrateKbps { get; init; }

        private StreamingQuality(string resolution, int bitrateKbps)
        {
            Resolution = resolution;
            BitrateKbps = bitrateKbps;
        }

        public static StreamingQuality P480 => new("480p", 1500);
        public static StreamingQuality P720 => new("720p", 3000);
        public static StreamingQuality P1080 => new("1080p", 6000);

        public static IEnumerable<StreamingQuality> All => new[] { P480, P720, P1080 };
    }
}
