using Microsoft.AspNetCore.Http;
namespace Menro.Application.Features.Icons.DTOs
{
    public class AddIconDto
    {
        public IFormFile Icon { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}
