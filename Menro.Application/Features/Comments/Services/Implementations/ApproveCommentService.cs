using Menro.Application.Comments.Services.Interfaces;
using Menro.Application.Features.Comments.DTOs;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
namespace Menro.Application.Comments.Services.Implementations
{
    public class ApproveCommentService : IApproveCommentService
    {
        private readonly ICommentRepository _commentRepository;
        public ApproveCommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }
        public async Task<CommentActionResult> ApproveAsync(int restaurantId, ApproveCommentDto dto)
        {
            var ownerRestaurantId = await _commentRepository.GetRestaurantIdByCommentIdAsync(dto.CommentId);
            if (ownerRestaurantId == null) return CommentActionResult.NotFound;
            if (ownerRestaurantId != restaurantId) return CommentActionResult.Forbidden;

            var comment = await _commentRepository.GetByIdAsync(dto.CommentId);
            if (comment == null) return CommentActionResult.NotFound;

            comment.Status = CommentStatus.Approved;
            comment.RejectReason = null;
            if (!string.IsNullOrWhiteSpace(dto.ReplyText))
            {
                comment.ReplyText = dto.ReplyText.Trim();
                comment.ReplyDate = DateTime.UtcNow;
            }
            await _commentRepository.SaveChangesAsync();
            return CommentActionResult.Success;
        }
    }
}