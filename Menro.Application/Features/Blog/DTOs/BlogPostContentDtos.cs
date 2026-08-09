namespace Menro.Application.Features.Blog.DTOs
{
    public record BlogPostContentResponse(Guid BlogPostId, string Content);
    public record BlogContentImageUploadResponse(string Url);
    public class UpdateBlogPostContentRequest
    {
        public string Content { get; set; } = string.Empty;
    }

}
