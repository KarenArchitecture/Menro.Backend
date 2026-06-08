using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Music.DTOs
{
    public class UploadMusicCoverDto
    {
        public IFormFile File { get; set; }
    }
}
