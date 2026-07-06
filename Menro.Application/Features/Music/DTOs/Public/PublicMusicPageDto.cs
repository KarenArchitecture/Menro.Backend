namespace Menro.Application.Features.Music.DTOs.Public
{
    public class PublicMusicPageDto
    {
        public int RemainingRequests { get; set; }

        public Guid? CurrentTrackId { get; set; }

        public List<PublicTrackDto> Tracks { get; set; }
    }
}
