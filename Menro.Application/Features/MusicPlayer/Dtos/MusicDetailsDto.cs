namespace Menro.Application.Features.MusicPlayer.Dtos
{
    public class MusicDetailsDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
        public string Artist { get; set; }

        public TimeSpan Duration { get; set; }

        public string FileUrl { get; set; }
        public string? CoverUrl { get; set; }

        public bool IsActive { get; set; }
    }
}
