using Menro.Domain.Entities.SiteContent;
using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Features.SiteContent.DTOs
{
    public class CreateMenuItemDto
    {
        [Required]
        public MenuLocation Location { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Url { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public Guid? ParentId { get; set; }
        // Order عمداً اینجا نیست - بک‌اند خودش آخرین Order + 1 رو محاسبه می‌کنه
    }
}