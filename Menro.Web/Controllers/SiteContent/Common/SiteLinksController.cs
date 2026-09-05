using Menro.Application.Features.SiteContent.DTOs;
using Menro.Application.Features.SiteContent.Services.Interfaces;
using Menro.Domain.Entities.SiteContent;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.SiteContent.Common
{
    [ApiController]
    [Route("api/site-content/links")]
    public class SiteLinksController : ApiControllerBase
    {
        private readonly ISiteLinkService _menuItemService;

        public SiteLinksController(ISiteLinkService menuItemService)
        {
            _menuItemService = menuItemService;
        }

        /// <summary>منوی فعال یک بخش (Header/Footer/Hamburger) برای نمایش در فرانت.</summary>
        [HttpGet("{location}")]
        public async Task<ActionResult<List<SiteLinkDto>>> GetByLocation(MenuLocation location)
        {
            var result = await _menuItemService.GetPublicMenuAsync(location);
            return Ok(result);
        }
    }
}
