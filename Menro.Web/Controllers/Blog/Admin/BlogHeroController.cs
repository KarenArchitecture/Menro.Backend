using Menro.Application.Common.SD;
using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Features.Blog.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Menro.Web.Controllers.Blog.Admin
{
    [ApiController]
    [Authorize]
    [Route("api/admin/blog/hero")]
    public class BlogHeroController : ApiControllerBase
    {
        private readonly IBlogHeroService _service;
        public BlogHeroController(IBlogHeroService service)
        {
            _service = service;
        }
        [HttpGet]
        [Authorize(Roles = SD.Roles_ContributorUp)]
        public async Task<ActionResult<BlogHeroResponse>> Get(CancellationToken ct)
        {
            return Ok(await _service.GetAsync(ct));
        }
        [HttpPut]
        [Authorize(Roles = SD.Roles_EditorUp)]
        public async Task<ActionResult<BlogHeroResponse>> Update(
            [FromBody] UpdateBlogHeroRequest request, CancellationToken ct)
        {
            return Ok(await _service.UpdateAsync(request, ct));
        }
    }
}