using System.ComponentModel.DataAnnotations;

namespace Menro.Application.DTOs.Blog
{
    // ArticleCount is read-only/derived - it never appears on the create/update
    // requests below, only on the response.
    public record BlogTagResponse(
        Guid Id,
        string Name,
        int ArticleCount);

    public class CreateBlogTagRequest
    {
        [Required(ErrorMessage = "نام برچسب الزامی است.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateBlogTagRequest
    {
        [Required(ErrorMessage = "نام برچسب الزامی است.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
