// Web/Controllers/User/CommentController.cs
using Menro.Application.Comments.Services.Interfaces;
using Menro.Application.Features.Comments.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Menro.Web.Controllers.Comment
{
    [ApiController]
    [Route("api/user/comment")]
    public class UserCommentController : ApiControllerBase
    {
        private readonly IGetFoodCommentsService _getFoodCommentsService;
        private readonly ICreateCommentService _createCommentService;
        private readonly IToggleCommentLikeService _toggleCommentLikeService;
        private readonly IGetMyCommentsService _getMyCommentsService;

        public UserCommentController(
            IGetFoodCommentsService getFoodCommentsService,
            ICreateCommentService createCommentService,
            IToggleCommentLikeService toggleCommentLikeService,
            IGetMyCommentsService getMyCommentsService)
        {
            _getFoodCommentsService = getFoodCommentsService;
            _createCommentService = createCommentService;
            _toggleCommentLikeService = toggleCommentLikeService;
            _getMyCommentsService = getMyCommentsService;
        }

        // GET: /api/user/comment/food/{foodId}  — guests allowed
        [HttpGet("food/{foodId}")]
        [AllowAnonymous]
        public async Task<ActionResult<FoodCommentsResponseDto>> GetByFood(int foodId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var data = await _getFoodCommentsService.GetCommentsByFoodIdAsync(foodId, userId);
            if (data == null) return NotFound("غذا یافت نشد.");
            return Ok(data);
        }

        // GET: /api/user/comment/my  — login required
        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult<List<MyCommentDto>>> GetMy()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("کاربر شناسایی نشد.");

            var comments = await _getMyCommentsService.GetMyCommentsAsync(userId);
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
            if (!success) return BadRequest(error);

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
            if (result == null) return NotFound("نظر یافت نشد.");

            return Ok(result);
        }
    }
}