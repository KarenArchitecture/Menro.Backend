using Menro.Application.Common.SD;
using Menro.Application.Features.Music.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Menro.Application.Common.Interfaces;

namespace Menro.Web.Controllers.AdminPanel.Music
{
    [Authorize (Roles = SD.Role_Owner)]
    [ApiController]
    [Route("api/admin/music/requests")]
    public class TrackRequestController : ControllerBase
    {
        private readonly ITrackRequestService _trackRequestService;
        private readonly ICurrentUserService _currentUserService;

        public TrackRequestController(ITrackRequestService rackRequestService,
            ICurrentUserService currentUserService)
        {
            _trackRequestService = rackRequestService;
            _currentUserService = currentUserService;
        }


        [HttpGet]
        public async Task<IActionResult> GetPending()
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var requests = await _trackRequestService.GetPendingAsync(restaurantId);

            return Ok(requests);
        }

        [HttpPost("{requestId:guid}/reject")]
        public async Task<IActionResult> Reject(Guid requestId)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();

            var result = await _trackRequestService.RejectAsync(requestId, restaurantId);

            if (!result)
                return NotFound();

            return Ok();
        }

        [HttpPost("{requestId:guid}/approve")]
        public async Task<IActionResult> Approve(Guid requestId)
        {
            var restaurantId =
                await _currentUserService.GetRestaurantIdAsync();

            var result =
                await _trackRequestService
                    .ApproveAsync(requestId, restaurantId);

            if (!result)
            {
                return NotFound(new
                {
                    message = "درخواست یافت نشد یا قابل تأیید نیست."
                });
            }

            return Ok(new
            {
                message = "درخواست با موفقیت تأیید شد."
            });
        }
    }
}
