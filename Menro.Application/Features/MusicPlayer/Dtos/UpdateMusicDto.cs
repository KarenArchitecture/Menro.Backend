namespace Menro.Application.Features.MusicPlayer.Dtos
{
    public class UpdateMusicDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
        public string Artist { get; set; }

        public bool IsActive { get; set; }
    }
}
