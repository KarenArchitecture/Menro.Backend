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
    public class BlogPostsController : ControllerBase
    {
        private readonly IBlogPostService _service;
        private readonly IFileService _fileService;
        private readonly IFileUrlService _fileUrlService;

        public BlogPostsController(IBlogPostService service, IFileService fileService, IFileUrlService fileUrlService)
        {
            _service = service;
            _fileService = fileService;
            _fileUrlService = fileUrlService;
        }

        // GET api/admin/blog/posts?search=...&categoryId=...&tagId=...&page=1&pageSize=20
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

        // POST api/admin/blog/posts/cover-image
        [HttpPost("cover-image")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<object>> UploadCoverImage(
            [FromForm] IFormFile file,
            [FromForm] string? oldFileName,
            CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                return BadRequest("فایلی ارسال نشده است.");

            var fileName = await _fileService.UploadBlogPostImageAsync(file, oldFileName);
            var url = _fileUrlService.BuildBlogPostImageUrl(fileName);

            return Ok(new { fileName, url });
        }

        // POST api/admin/blog/posts
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
