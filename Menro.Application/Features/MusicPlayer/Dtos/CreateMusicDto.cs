using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.MusicPlayer.Dtos
{
    public class CreateMusicDto
    {
        public string Title { get; set; }
        public string Artist { get; set; }

        public IFormFile MusicFile { get; set; }
        public IFormFile? CoverFile { get; set; }
    }
}
