using Menro.Application.Common.Interfaces;
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
        public async Task<ActionResult<PagedResult<BlogPostResponse>>> GetAll(
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
        public async Task<ActionResult<BlogPostResponse>> GetById(Guid id, CancellationToken ct)
        {
            var post = await _service.GetByIdAsync(id, ct);
            return post is null ? NotFound() : Ok(post);
        }

        // POST api/admin/blog/posts - multipart: فیلدها + عکس (اختیاری) با هم
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<BlogPostResponse>> Create(
            [FromForm] CreateBlogPostRequest request, CancellationToken ct)
        {
            var created = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT api/admin/blog/posts/{id} - multipart: فیلدها + عکس (اختیاری) + RemoveImage با هم
        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<BlogPostResponse>> Update(
            Guid id, [FromForm] UpdateBlogPostRequest request, CancellationToken ct)
        {
            var updated = await _service.UpdateAsync(id, request, ct);
            return updated is null ? NotFound() : Ok(updated);
        }

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