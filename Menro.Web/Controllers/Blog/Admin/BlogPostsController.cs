using Menro.Application.Common.SD;
using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Features.Blog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Blog.Admin
{
    [ApiController]
    [Authorize(Roles = SD.Role_Admin)] // add author role for later on
    [Route("api/admin/blog/posts")]
    public class BlogPostsController : ApiControllerBase
    {
        #region DI
        private readonly IBlogPostService _service;
        public BlogPostsController(IBlogPostService service)
        {
            _service = service;
        }
        #endregion

        [HttpGet]
        public async Task<ActionResult<PagedResult<BlogPostListItemResponse>>> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? categoryId,
            [FromQuery] Guid? tagId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var result = await _service.GetAllAsync(
                search, categoryId, tagId, page: page, pageSize: pageSize, ct: ct);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BlogPostDetailResponse>> GetById(Guid id, CancellationToken ct)
        {
            var post = await _service.GetByIdAsync(id, ct);
            return post is null ? NotFound() : Ok(post);
        }

        // POST api/admin/blog/posts - ساخت پیش‌نویس: فقط Title، بدون فایل
        [HttpPost]
        public async Task<ActionResult<BlogPostDetailResponse>> Create(
            [FromBody] CreateBlogPostRequest request, CancellationToken ct)
        {
            var created = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT api/admin/blog/posts/{id}
        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<BlogPostDetailResponse>> Update(
            Guid id, [FromForm] UpdateBlogPostRequest request, CancellationToken ct)
        {
            var updated = await _service.UpdateAsync(id, request, ct);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpPatch("{id:guid}/publish")]
        public async Task<ActionResult<BlogPostPublishResponse>> TogglePublish(Guid id, CancellationToken ct)
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

        /* --- BLOG CONTENT --- */
        [HttpGet("{id:guid}/content")]
        public async Task<ActionResult<BlogPostContentResponse>> GetContent(Guid id, CancellationToken ct)
        {
            var content = await _service.GetContentAsync(id, ct);
            return content is null ? NotFound() : Ok(content);
        }

        [HttpPut("{id:guid}/content")]
        public async Task<ActionResult<BlogPostContentResponse>> UpdateContent(
            Guid id, [FromBody] UpdateBlogPostContentRequest request, CancellationToken ct)
        {
            var updated = await _service.UpdateContentAsync(id, request, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
    }
}