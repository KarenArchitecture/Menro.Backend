using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Features.Blog.Services;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Api.Controllers
{
    [ApiController]
    [Route("api/admin/blog/hero")]
    public class BlogHeroController : ControllerBase
    {
        private readonly IBlogHeroService _service;

        public BlogHeroController(IBlogHeroService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<BlogHeroResponse>> Get(CancellationToken ct)
        {
            return Ok(await _service.GetAsync(ct));
        }

        [HttpPut]
        public async Task<ActionResult<BlogHeroResponse>> Update(
            [FromBody] UpdateBlogHeroRequest request, CancellationToken ct)
        {
            return Ok(await _service.UpdateAsync(request, ct));
        }
    }
}
