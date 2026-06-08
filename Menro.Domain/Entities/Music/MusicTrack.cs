namespace Menro.Domain.Entities.Music
{
    public class MusicTrack
    {
        public Guid Id { get; set; }

        public int RestaurantId { get; set; }

        public string Title { get; set; }

        public string Artist { get; set; }

        public TimeSpan Duration { get; set; }

        public string AudioFileName { get; set; }

        public string? CoverFileName { get; set; }

        public bool IsActive { get; set; }
    }
}