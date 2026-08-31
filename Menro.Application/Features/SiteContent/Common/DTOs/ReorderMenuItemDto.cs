namespace Menro.Application.Features.SiteContent.DTOs
{
    /// <summary>
    /// خروجی درگ‌اند‌دراپ پنل ادمین: لیست کامل Id ها به ترتیب جدید برای یک Location.
    /// </summary>
    public class ReorderMenuItemDto
    {
        public List<Guid> OrderedIds { get; set; } = new();
    }
}
