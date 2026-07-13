using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Features.Blog.Services;
using Menro.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Blog
{
    [ApiController]
    [Route("api/blog/posts")]
    public class PublicBlogPostsController : ControllerBase
    {
        private readonly IBlogPostService _service;

        public PublicBlogPostsController(IBlogPostService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<BlogPostResponse>>> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? categoryId,
            [FromQuery] BlogPostSortOrder sort = BlogPostSortOrder.Newest,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            CancellationToken ct = default)
        {
            var result = await _service.GetAllAsync(
                search, categoryId, sort, publishedOnly: true, page, pageSize, ct);
            return Ok(result);
        }
    }
}