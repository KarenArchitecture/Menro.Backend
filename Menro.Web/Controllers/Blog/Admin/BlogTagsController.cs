using Menro.Application.Common.SD;
using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Features.Blog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Blog.Admin
{
    [ApiController]
    [Authorize(Roles = SD.Role_Admin)] // add author role for later on
    [Route("api/admin/blog/sidebar-tags")]
    public class BlogTagsController : ControllerBase
    {
        private readonly IBlogTagService _service;

        public BlogTagsController(IBlogTagService service)
        {
            _service = service;
        }

        // Article counts are always computed server-side and returned here -
        // the create/update request bodies never accept a count.
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<BlogTagResponse>>> GetAll(CancellationToken ct)
        {
            return Ok(await _service.GetAllAsync(ct));
        }

        [HttpPost]
        public async Task<ActionResult<BlogTagResponse>> Create(
            [FromBody] CreateBlogTagRequest request, CancellationToken ct)
        {
            try
            {
                var created = await _service.CreateAsync(request, ct);
                return Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<BlogTagResponse>> Update(
            Guid id, [FromBody] UpdateBlogTagRequest request, CancellationToken ct)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, request, ct);
                return updated is null ? NotFound() : Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // Mirrors POST /api/admin/blog/posts/{id}/toggle-publish
        [HttpPatch("{id:guid}/toggle-suggested")]
        public async Task<ActionResult<BlogTagResponse>> ToggleSuggested(Guid id, CancellationToken ct)
        {
            try
            {
                var updated = await _service.ToggleSuggestedAsync(id, ct);
                return updated is null ? NotFound() : Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var deleted = await _service.DeleteAsync(id, ct);
            return deleted ? NoContent() : NotFound();
        }
    }
}
