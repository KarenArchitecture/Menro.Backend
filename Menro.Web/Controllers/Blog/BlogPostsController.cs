using Menro.Application.DTOs.Blog;
using Menro.Application.Features.Blog.Services;
using Menro.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Api.Controllers
{
    [ApiController]
    [Route("api/admin/blog/posts")]
    public class BlogPostsController : ControllerBase
    {
        private readonly IBlogPostService _service;

        public BlogPostsController(IBlogPostService service)
        {
            _service = service;
        }

        // GET api/admin/blog/posts?search=...&category=Newest
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<BlogPostResponse>>> GetAll(
            [FromQuery] string? search, [FromQuery] BlogFeedCategory? category, CancellationToken ct)
        {
            var posts = await _service.GetAllAsync(search, category, ct);
            return Ok(posts);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BlogPostResponse>> GetById(Guid id, CancellationToken ct)
        {
            var post = await _service.GetByIdAsync(id, ct);
            return post is null ? NotFound() : Ok(post);
        }

        [HttpPost]
        public async Task<ActionResult<BlogPostResponse>> Create(
            [FromBody] CreateBlogPostRequest request, CancellationToken ct)
        {
            var created = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<BlogPostResponse>> Update(
            Guid id, [FromBody] UpdateBlogPostRequest request, CancellationToken ct)
        {
            var updated = await _service.UpdateAsync(id, request, ct);
            return updated is null ? NotFound() : Ok(updated);
        }

        // PATCH api/admin/blog/posts/{id}/publish - toggles published/draft, mirrors
        // the click-to-toggle status chip in the admin table.
        [HttpPatch("{id:guid}/publish")]
        public async Task<ActionResult<BlogPostResponse>> TogglePublish(Guid id, CancellationToken ct)
        {
            var updated = await _service.TogglePublishAsync(id, ct);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var deleted = await _service.DeleteAsync(id, ct);
            return deleted ? NoContent() : NotFound();
        }
    }
}
