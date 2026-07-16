using Menro.Application.Common.Interfaces;
using Menro.Application.Features.Favorites.Services.Interfaces;
using Menro.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Favorites
{
    [ApiController]
    [Route("api/user/favorites")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteFoodService _favoriteService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public FavoritesController(
            IFavoriteFoodService favoriteService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _favoriteService = favoriteService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        // -------------------------
        // Toggle favorite (heart click)
        // -------------------------
        [HttpPost("{foodId:int}")]
        public async Task<IActionResult> Toggle(int foodId)
        {
            var userId = _currentUserService.GetUserId();

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            await _favoriteService.ToggleAsync(userId, foodId);

            await _unitOfWork.SaveChangesAsync();

            return Ok(new
            {
                message = "Favorite updated"
            });
        }

        // -------------------------
        // Get all favorites
        // -------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = _currentUserService.GetUserId();

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await _favoriteService.GetUserFavoritesAsync(userId);

            return Ok(result);
        }

        // -------------------------
        // Get favorite IDs only (lightweight)
        // -------------------------
        [HttpGet("ids")]
        public async Task<IActionResult> GetIds()
        {
            var userId = _currentUserService.GetUserId();

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await _favoriteService.GetFavoriteFoodIdsAsync(userId);

            return Ok(result);
        }
    }
}