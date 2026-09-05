using Menro.Application.Comments.Services.Interfaces;
using Menro.Application.Features.Comments.DTOs;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
namespace Menro.Application.Comments.Services.Implementations
{
    public class RejectCommentService : IRejectCommentService
    {
        private readonly ICommentRepository _commentRepository;
        public RejectCommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }
        public async Task<CommentActionResult> RejectAsync(int restaurantId, RejectCommentDto dto)
        {
            var ownerRestaurantId = await _commentRepository.GetRestaurantIdByCommentIdAsync(dto.CommentId);
            if (ownerRestaurantId == null) return CommentActionResult.NotFound;
            if (ownerRestaurantId != restaurantId) return CommentActionResult.Forbidden;

            var comment = await _commentRepository.GetByIdAsync(dto.CommentId);
            if (comment == null) return CommentActionResult.NotFound;

            comment.Status = CommentStatus.Rejected;
            comment.RejectReason = dto.Reason?.Trim();
            comment.ReplyText = null;
            comment.ReplyDate = null;
            await _commentRepository.SaveChangesAsync();
            return CommentActionResult.Success;
        }
    }
}