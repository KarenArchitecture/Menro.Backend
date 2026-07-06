using Menro.Application.Features.Music.Enums;

namespace Menro.Application.Features.Music.DTOs.Public
{
    public class PublicTrackDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsCurrentTrack { get; set; }

        public PublicTrackStatus Status { get; set; }
    }
}
