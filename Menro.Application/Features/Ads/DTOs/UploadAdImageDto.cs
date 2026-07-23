using Menro.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Ads.DTOs
{
    public class UploadAdImageDto
    {
        public IFormFile File { get; set; } = default!;
        public AdPlacementType PlacementType { get; set; }
    }
}