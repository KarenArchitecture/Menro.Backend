using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Features.Blog.Services;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Blog
{
    [ApiController]
    [Route("api/blog/display-categories")]
    public class PublicBlogCategoriesController : ControllerBase
    {
        private readonly IBlogCategoryService _service;

        public PublicBlogCategoriesController(IBlogCategoryService service)
        {
            _service = service;
        }

        // get blog category cards
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<BlogCategoryResponse>>> GetAll(CancellationToken ct)
        {
            // Assumes GetAllAsync already returns items ordered by SortOrder
            return Ok(await _service.GetAllAsync(ct));
        }
    }
}
