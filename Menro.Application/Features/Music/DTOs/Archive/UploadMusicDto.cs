using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Music.DTOs.Archive
{
    public class UploadMusicDto
    {
        public IFormFile File { get; set; }
    }
}
