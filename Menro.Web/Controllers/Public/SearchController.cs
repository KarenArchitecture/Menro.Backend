using Menro.Application.Features.Search.DTOs;
using Menro.Application.Features.Search.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Public
{
    [ApiController]
    [Route("api/public/search")]
    public class SearchController : ApiControllerBase
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

        [HttpGet("paged")]
        public async Task<IActionResult> SearchPaged(
            [FromQuery] string term,
            [FromQuery] SearchItemType type,
            [FromQuery] string? cursor = null,
            [FromQuery] int take = 12,
            [FromQuery] int? categoryId = null)
        {
            var skip = int.TryParse(cursor, out var s) && s > 0 ? s : 0;
            var res = await _service.SearchPagedAsync(term, type, skip, take, categoryId);
            return Ok(res);
        }
    }
}
