using Menro.Application.Common.SD;
using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Features.Blog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Blog.Admin
{
    [ApiController]
    [Authorize(Roles = SD.Role_Admin)] // add author role for later on
    [Route("api/admin/blog/hero")]
    public class BlogHeroController : ApiControllerBase
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
