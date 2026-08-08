using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Features.Blog.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Blog.Public
{
    [ApiController]
    [Route("api/public/blog/page-bootstrap")]
    public class PublicBlogPageBootstrapController : ApiControllerBase
    {
        #region DI

        private readonly IBlogHeroService _heroService;
        private readonly IBlogCategoryService _categoryService;
        private readonly IBlogTagService _tagService;

        public PublicBlogPageBootstrapController(
            IBlogHeroService heroService,
            IBlogCategoryService categoryService,
            IBlogTagService tagService)
        {
            _heroService = heroService;
            _categoryService = categoryService;
            _tagService = tagService;
        }

        #endregion

        // get Hero, categories and tags for public blog page
        [HttpGet]
        public async Task<ActionResult<BlogPageBootstrapResponse>> Get(CancellationToken ct)
        {
            var hero = await _heroService.GetAsync(ct);
            var categories = await _categoryService.GetAllAsync(ct);
            var tags = await _tagService.GetSuggestedAsync(ct);

            return Ok(new BlogPageBootstrapResponse
            {
                Hero = hero,
                Categories = categories,
                SidebarTags = tags,
            });
        }
    }
}