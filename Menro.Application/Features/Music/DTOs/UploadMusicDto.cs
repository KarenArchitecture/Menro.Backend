using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Music.DTOs
{
    public class UploadMusicDto
    {
        public IFormFile File { get; set; }
    }
}
