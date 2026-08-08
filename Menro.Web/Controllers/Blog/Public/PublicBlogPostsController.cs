using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Features.Blog.Services.Interfaces;
using Menro.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Blog.Public
{
    [ApiController]
    [Route("api/public/blog/posts")]
    public class PublicBlogPostsController : ApiControllerBase
    {
        #region DI
        private readonly IBlogPostService _postService;
        private readonly IBlogCategoryService _categoryService;
        private readonly IBlogTagService _tagService;

        public PublicBlogPostsController(
            IBlogPostService postService,
            IBlogCategoryService categoryService,
            IBlogTagService tagService)
        {
            _postService = postService;
            _categoryService = categoryService;
            _tagService = tagService;
        }
        #endregion

        [HttpGet]
        public async Task<ActionResult<PagedResult<BlogPostListItemResponse>>> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? categorySlug,
            [FromQuery] string? tagSlug,
            [FromQuery] BlogPostSortOrder sort = BlogPostSortOrder.Newest,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            CancellationToken ct = default)
        {
            Guid? categoryId = null;
            if (!string.IsNullOrWhiteSpace(categorySlug))
            {
                var category = await _categoryService.GetBySlugAsync(categorySlug, ct);
                if (category is null)
                    return NotFound(new { message = "دسته‌بندی مورد نظر یافت نشد." });
                categoryId = category.Id;
            }

            Guid? tagId = null;
            if (!string.IsNullOrWhiteSpace(tagSlug))
            {
                var tag = await _tagService.GetBySlugAsync(tagSlug, ct);
                if (tag is null)
                    return NotFound(new { message = "برچسب مورد نظر یافت نشد." });
                tagId = tag.Id;
            }

            var result = await _postService.GetAllAsync(
                search, categoryId, tagId, sort, publishedOnly: true, page, pageSize, ct);

            return Ok(result);
        }


        [HttpGet("{slug}")]
        public async Task<ActionResult<BlogPostPublicDetailResponse>> GetBySlug(string slug, CancellationToken ct)
        {
            var post = await _postService.GetPublicBySlugAsync(slug, ct);
            if (post is null) return NotFound();
            return Ok(post);
        }

        [HttpGet("{slug}/popular")]
        public async Task<ActionResult<IReadOnlyList<BlogPostRelatedItemResponse>>> GetPopular(string slug, [FromQuery] int count = 5, CancellationToken ct = default)
        {
            var result = await _postService.GetPopularPostsAsync(slug, count, ct);
            return Ok(result);
        }

        [HttpGet("{slug}/related")]
        public async Task<ActionResult<IReadOnlyList<BlogPostRelatedItemResponse>>> GetRelated(string slug, [FromQuery] int count = 3, CancellationToken ct = default)
        {
            var result = await _postService.GetRelatedPostsAsync(slug, count, ct);
            return Ok(result);
        }
    }
}
