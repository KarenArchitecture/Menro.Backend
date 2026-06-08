using TagLib;
using System;
using System.IO;

namespace Menro.Application.Helpers
{
    public class AudioMetadata
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public static class AudioMetadataExtractor
    {
        public static AudioMetadata Extract(string filePath)
        {
            try
            {
                using var file = TagLib.File.Create(filePath);

                var duration = GetSafeDuration(file);

                return new AudioMetadata
                {
                    Title = string.IsNullOrWhiteSpace(file.Tag.Title)
                        ? Path.GetFileNameWithoutExtension(filePath)
                        : file.Tag.Title,

                    Artist = file.Tag.FirstPerformer ?? "Unknown Artist",

                    Duration = duration
                };
            }
            catch
            {
                // fallback کامل اگر TagLib fail کرد
                return new AudioMetadata
                {
                    Title = Path.GetFileNameWithoutExtension(filePath),
                    Artist = "Unknown Artist",
                    Duration = TimeSpan.Zero
                };
            }
        }

        private static TimeSpan GetSafeDuration(TagLib.File file)
        {
            try
            {
                var duration = file.Properties.Duration;

                // fallback sanity check
                if (duration <= TimeSpan.Zero)
                    return EstimateDurationFallback(file);

                return duration;
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        private static TimeSpan EstimateDurationFallback(TagLib.File file)
        {
            try
            {
                // fallback 1: bitrate-based estimation
                if (file.Properties.AudioBitrate > 0)
                {
                    var fileSizeBytes = file.Length;
                    var bitrate = file.Properties.AudioBitrate * 1000; // kbps → bps

                    var seconds = fileSizeBytes * 8.0 / bitrate;

                    if (seconds > 0 && seconds < 24 * 60 * 60) // sanity cap 24h
                        return TimeSpan.FromSeconds(seconds);
                }
            }
            catch
            {
                // ignore
            }

            return TimeSpan.Zero;
        }

        public static byte[]? ExtractCover(string filePath)
        {
            try
            {
                using var file = TagLib.File.Create(filePath);

                if (file.Tag.Pictures == null ||
                    file.Tag.Pictures.Length == 0)
                {
                    return null;
                }

                var picture = file.Tag.Pictures[0];

                return picture?.Data?.Data;
            }
            catch
            {
                return null;
            }
        }
    }
}