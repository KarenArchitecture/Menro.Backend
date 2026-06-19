using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Features.Music.DTOs.Archive
{
    public class UploadMusicTrackDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(200)]
        public string? Artist { get; set; }

        [Required]
        public IFormFile AudioFile { get; set; }

        public IFormFile? CoverFile { get; set; }
    }
}
