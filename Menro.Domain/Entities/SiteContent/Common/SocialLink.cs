namespace Menro.Domain.Entities.SiteContent
{
    public class SocialLink
    {
        public Guid Id { get; set; }
        public string Platform { get; set; } = string.Empty; // "instagram", "telegram", ...
        public string Url { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsActive { get; set; }
    }
}
