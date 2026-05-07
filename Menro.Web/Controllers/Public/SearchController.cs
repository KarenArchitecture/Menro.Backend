using Menro.Application.Features.Search.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Public
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/public/search")]
    public class SearchController : ControllerBase
    {
        private readonly IPublicSearchService _service;

        public SearchController(IPublicSearchService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string term, [FromQuery] int take = 15)
        {
            var res = await _service.SearchAsync(term, take);
            return Ok(res);
        }
    }
}
