using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Features.SiteContent.DTOs
{
    public class UpdateMenuItemDto
    {
        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Url { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public Guid? ParentId { get; set; }
        // Location و Order از این مسیر تغییر نمی‌کنن؛ Reorder مسیر جداست
    }
}