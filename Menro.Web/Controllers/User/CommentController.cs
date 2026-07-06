using Menro.Application.Comments.Services.Interfaces;
using Menro.Application.DTO;
using Menro.Application.Features.Comments.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Menro.Web.Controllers.User
{
    [ApiController]
    [Route("api/user/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly IGetFoodCommentsService _getFoodCommentsService;
        private readonly ICreateCommentService _createCommentService;
        private readonly IToggleCommentLikeService _toggleCommentLikeService;

        public CommentController(
            IGetFoodCommentsService getFoodCommentsService,
            ICreateCommentService createCommentService,
            IToggleCommentLikeService toggleCommentLikeService)
        {
            _getFoodCommentsService = getFoodCommentsService;
            _createCommentService = createCommentService;
            _toggleCommentLikeService = toggleCommentLikeService;
        }

        // GET: /api/user/comment/food/{foodId}  — guests allowed
        [HttpGet("food/{foodId}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<CommentDto>>> GetByFood(int foodId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comments = await _getFoodCommentsService.GetCommentsByFoodIdAsync(foodId, userId);
            return Ok(comments);
        }

        // POST: /api/user/comment  — login required
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] CreateCommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("کاربر شناسایی نشد.");

            var (success, error) = await _createCommentService.CreateCommentAsync(userId, dto);
            if (!success)
                return BadRequest(error);

            return Ok(new { message = "نظر شما ثبت شد و پس از تایید نمایش داده می‌شود." });
        }

        // POST: /api/user/comment/like  — login required
        [HttpPost("like")]
        [Authorize]
        public async Task<ActionResult<ToggleLikeResultDto>> ToggleLike([FromBody] ToggleCommentLikeDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("کاربر شناسایی نشد.");

            var result = await _toggleCommentLikeService.ToggleLikeAsync(userId, dto);
            if (result == null)
                return NotFound("نظر یافت نشد.");

            return Ok(result);
        }
    }
}