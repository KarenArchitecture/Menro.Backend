using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Music.DTOs.Archive
{
    public class UploadMusicCoverDto
    {
        public IFormFile File { get; set; }
    }
}
