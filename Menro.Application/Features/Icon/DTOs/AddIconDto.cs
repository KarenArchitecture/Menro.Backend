using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Features.Icons.DTOs
{
    public class AddIconDto
    {
        public string FileName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
