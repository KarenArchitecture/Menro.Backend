namespace Menro.Application.Features.Icons.DTOs
{
    public class GetIconDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? Label { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}
