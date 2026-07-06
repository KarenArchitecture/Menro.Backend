using Menro.Application.Comments.Services.Interfaces;
using Menro.Application.DTO;
using Menro.Application.Features.Comments.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers.AdminPanel
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CommentController : ControllerBase
    {
        private readonly IGetCommentsForAdminService _getCommentsForAdminService;
        private readonly IApproveCommentService _approveCommentService;
        private readonly IRejectCommentService _rejectCommentService;

        public CommentController(
            IGetCommentsForAdminService getCommentsForAdminService,
            IApproveCommentService approveCommentService,
            IRejectCommentService rejectCommentService)
        {
            _getCommentsForAdminService = getCommentsForAdminService;
            _approveCommentService = approveCommentService;
            _rejectCommentService = rejectCommentService;
        }

        // GET: /api/admin/comment?status=pending
        [HttpGet]
        public async Task<ActionResult<List<CommentAdminDto>>> GetComments([FromQuery] string status = "pending")
        {
            var comments = await _getCommentsForAdminService.GetCommentsAsync(status);
            return Ok(comments);
        }

        // POST: /api/admin/comment/approve
        [HttpPost("approve")]
        public async Task<ActionResult> Approve([FromBody] ApproveCommentDto dto)
        {
            var success = await _approveCommentService.ApproveAsync(dto);
            if (!success) return NotFound("نظر یافت نشد.");
            return Ok(new { message = "نظر تایید شد." });
        }

        // POST: /api/admin/comment/reject
        [HttpPost("reject")]
        public async Task<ActionResult> Reject([FromBody] RejectCommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest("ثبت دلیل برای رد نظر الزامی است.");

            var success = await _rejectCommentService.RejectAsync(dto);
            if (!success) return NotFound("نظر یافت نشد.");
            return Ok(new { message = "نظر رد شد." });
        }
    }
}