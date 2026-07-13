using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Features.Blog.Services;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Api.Controllers
{
    [ApiController]
    [Route("api/admin/blog/display-categories")]
    public class BlogCategoriesController : ControllerBase
    {
        private readonly IBlogCategoryService _service;

        public BlogCategoriesController(IBlogCategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<BlogCategoryResponse>>> GetAll(CancellationToken ct)
        {
            return Ok(await _service.GetAllAsync(ct));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BlogCategoryResponse>> GetById(Guid id, CancellationToken ct)
        {
            var category = await _service.GetByIdAsync(id, ct);
            return category is null ? NotFound() : Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<BlogCategoryResponse>> Create(
            [FromBody] CreateBlogCategoryRequest request, CancellationToken ct)
        {
            var created = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<BlogCategoryResponse>> Update(
            Guid id, [FromBody] UpdateBlogCategoryRequest request, CancellationToken ct)
        {
            var updated = await _service.UpdateAsync(id, request, ct);
            return updated is null ? NotFound() : Ok(updated);
        }

        // POST api/admin/blog/display-categories/{id}/move - the up/down reorder buttons.
        [HttpPost("{id:guid}/move")]
        public async Task<ActionResult<IReadOnlyList<BlogCategoryResponse>>> Move(
            Guid id, [FromBody] MoveBlogCategoryRequest request, CancellationToken ct)
        {
            var result = await _service.MoveAsync(id, request.Direction, ct);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var deleted = await _service.DeleteAsync(id, ct);
            return deleted ? NoContent() : NotFound();
        }
    }
}
