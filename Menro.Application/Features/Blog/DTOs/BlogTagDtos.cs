using System.ComponentModel.DataAnnotations;

namespace Menro.Application.Features.Blog.DTOs
{
    // ArticleCount is read-only/derived - it never appears on the create/update
    // requests below, only on the response.
    public record BlogTagResponse(
        Guid Id,
        string Name,
        int ArticleCount,
        bool? Suggested);

    public class CreateBlogTagRequest
    {
        [Required(ErrorMessage = "نام برچسب الزامی است.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool? Suggested { get; set; }
    }

    public class UpdateBlogTagRequest
    {
        [Required(ErrorMessage = "نام برچسب الزامی است.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool? Suggested { get; set; }
    }
}
