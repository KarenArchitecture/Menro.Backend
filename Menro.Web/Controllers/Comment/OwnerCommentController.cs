using Menro.Application.Comments.Services.Interfaces;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.SD;
using Menro.Application.Features.Comments.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.Comment
{
    [ApiController]
    [Route("api/owner/comment")]
    [Authorize(Roles = SD.Role_Owner)]
    public class OwnerCommentController : ApiControllerBase
    {
        private readonly IGetCommentsForOwnerService _getCommentsForOwnerService;
        private readonly IApproveCommentService _approveCommentService;
        private readonly IRejectCommentService _rejectCommentService;
        private readonly ICurrentUserService _currentUserService;

        public OwnerCommentController(
            IGetCommentsForOwnerService getCommentsForOwnerService,
            IApproveCommentService approveCommentService,
            IRejectCommentService rejectCommentService,
            ICurrentUserService currentUserService)
        {
            _getCommentsForOwnerService = getCommentsForOwnerService;
            _approveCommentService = approveCommentService;
            _rejectCommentService = rejectCommentService;
            _currentUserService = currentUserService;
        }

        // GET: /api/owner/comment?status=pending
        [HttpGet]
        public async Task<ActionResult<List<CommentAdminDto>>> GetComments([FromQuery] string status = "pending")
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            if (restaurantId == 0) return Forbid();

            var comments = await _getCommentsForOwnerService.GetCommentsAsync(restaurantId, status);
            return Ok(comments);
        }

        // POST: /api/owner/comment/approve
        [HttpPost("approve")]
        public async Task<ActionResult> Approve([FromBody] ApproveCommentDto dto)
        {
            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            if (restaurantId == 0) return Forbid();

            var result = await _approveCommentService.ApproveAsync(restaurantId, dto);
            return result switch
            {
                CommentActionResult.NotFound => NotFound("نظر یافت نشد."),
                CommentActionResult.Forbidden => Forbid(),
                _ => Ok(new { message = "نظر تایید شد." })
            };
        }

        // POST: /api/owner/comment/reject
        [HttpPost("reject")]
        public async Task<ActionResult> Reject([FromBody] RejectCommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest("ثبت دلیل برای رد نظر الزامی است.");

            var restaurantId = await _currentUserService.GetRestaurantIdAsync();
            if (restaurantId == 0) return Forbid();

            var result = await _rejectCommentService.RejectAsync(restaurantId, dto);
            return result switch
            {
                CommentActionResult.NotFound => NotFound("نظر یافت نشد."),
                CommentActionResult.Forbidden => Forbid(),
                _ => Ok(new { message = "نظر رد شد." })
            };
        }
    }
}