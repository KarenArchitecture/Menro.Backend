namespace Menro.Application.Features.SiteContent.DTOs
{
    public class MenuItemDto
    {
        public Guid Id { get; set; }
        public string Location { get; set; } = string.Empty; // "Header" / "Footer" / "Hamburger"
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public Guid? ParentId { get; set; }
    }
}