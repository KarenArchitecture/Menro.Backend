using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Features.Blog.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Blog.Public
{
    // Read-only, public. Used by the frontend both for the "دسته‌بندی‌های
    // نمایشی" / "برچسب‌های پیشنهادی" tabs on the blog home page, and to fetch
    // the single category/tag (by slug) that powers the results-page header
    // ("دسته: ..." / "برچسب: ...") after PublicBlogPostsController resolves
    // the same slug for filtering.
    [ApiController]
    [Route("api/public/blog")]
    public class PublicBlogTaxonomyController : ApiControllerBase
    {
        private readonly IBlogCategoryService _categoryService;
        private readonly IBlogTagService _tagService;

        public PublicBlogTaxonomyController(IBlogCategoryService categoryService, IBlogTagService tagService)
        {
            _categoryService = categoryService;
            _tagService = tagService;
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IReadOnlyList<BlogCategoryResponse>>> GetCategories(CancellationToken ct)
        {
            var categories = await _categoryService.GetAllAsync(ct);
            return Ok(categories);
        }

        [HttpGet("categories/{slug}")]
        public async Task<ActionResult<BlogCategoryResponse>> GetCategoryBySlug(string slug, CancellationToken ct)
        {
            var category = await _categoryService.GetBySlugAsync(slug, ct);
            return category is null ? NotFound() : Ok(category);
        }

        [HttpGet("tags/suggested")]
        public async Task<ActionResult<IReadOnlyList<BlogTagResponse>>> GetSuggestedTags(CancellationToken ct)
        {
            var tags = await _tagService.GetSuggestedAsync(ct);
            return Ok(tags);
        }

        [HttpGet("tags/{slug}")]
        public async Task<ActionResult<BlogTagResponse>> GetTagBySlug(string slug, CancellationToken ct)
        {
            var tag = await _tagService.GetBySlugAsync(slug, ct);
            return tag is null ? NotFound() : Ok(tag);
        }
    }
}
