namespace Menro.Application.Features.Blog.DTOs
{ 
    public class BlogPageBootstrapResponse
    {
        public BlogHeroResponse Hero { get; set; } = null!;
        public IReadOnlyList<BlogCategoryResponse> Categories { get; set; } = new List<BlogCategoryResponse>();
        public IReadOnlyList<BlogTagResponse> SidebarTags { get; set; } = new List<BlogTagResponse>();
    }
}