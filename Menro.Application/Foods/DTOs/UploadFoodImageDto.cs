using Microsoft.AspNetCore.Http;

namespace Menro.Application.Foods.DTOs
{
    public class UploadFoodImageDto
    {
        public IFormFile File { get; set; } = default!;
    }
}
