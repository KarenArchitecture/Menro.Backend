namespace Menro.Domain.Entities
{
    public class Music
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public string Title { get; private set; }
        public string Artist { get; private set; }

        public TimeSpan Duration { get; private set; }

        public string FilePath { get; private set; }
        public string? CoverPath { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private Music() { } // For EF

        public Music(
            string title,
            string artist,
            TimeSpan duration,
            string filePath,
            string? coverPath = null)
        {
            Id = Guid.NewGuid();
            Title = title;
            Artist = artist;
            Duration = duration;
            FilePath = filePath;
            CoverPath = coverPath;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateInfo(string title, string artist)
        {
            Title = title;
            Artist = artist;
        }

        public void ChangeCover(string coverPath)
        {
            CoverPath = coverPath;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }
    }
}
