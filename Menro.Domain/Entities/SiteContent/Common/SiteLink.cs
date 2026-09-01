namespace Menro.Domain.Entities.SiteContent
{
    public enum MenuLocation
    {
        Header = 1,
        Footer = 2,
    }

    public class SiteLink
    {
        public Guid Id { get; set; }
        public MenuLocation Location { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsActive { get; set; }
        /// <summary>برای منوهای تو در تو (مثلاً dropdown توی هدر)، در فوتر فعلاً null.</summary>
        public Guid? ParentId { get; set; }
    }
}
