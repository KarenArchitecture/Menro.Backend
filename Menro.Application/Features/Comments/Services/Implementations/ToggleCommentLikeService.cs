using Menro.Application.Comments.Services.Interfaces;
using Menro.Application.Features.Comments.DTOs;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;

namespace Menro.Application.Comments.Services.Implementations
{
    public class ToggleCommentLikeService : IToggleCommentLikeService
    {
        private readonly ICommentRepository _commentRepository;

        public ToggleCommentLikeService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<ToggleLikeResultDto?> ToggleLikeAsync(string userId, ToggleCommentLikeDto dto)
        {
            var target = dto.Target?.ToLower() == "reply"
                ? CommentLikeTarget.Reply
                : CommentLikeTarget.Comment;

            var comment = await _commentRepository.GetByIdAsync(dto.CommentId);
            if (comment == null)
                return null;

            if (target == CommentLikeTarget.Reply && string.IsNullOrEmpty(comment.ReplyText))
                return null;

            var existingLike = await _commentRepository.GetLikeAsync(dto.CommentId, userId, target);
            bool liked;

            if (existingLike != null)
            {
                await _commentRepository.RemoveLikeAsync(existingLike);
                if (target == CommentLikeTarget.Comment)
                    comment.LikesCount = Math.Max(0, comment.LikesCount - 1);
                else
                    comment.ReplyLikesCount = Math.Max(0, comment.ReplyLikesCount - 1);
                liked = false;
            }
            else
            {
                await _commentRepository.AddLikeAsync(new CommentLike
                {
                    CommentId = dto.CommentId,
                    UserId = userId,
                    Target = target,
                    CreatedAt = DateTime.UtcNow
                });
                if (target == CommentLikeTarget.Comment)
                    comment.LikesCount += 1;
                else
                    comment.ReplyLikesCount += 1;
                liked = true;
            }

            await _commentRepository.SaveChangesAsync();

            return new ToggleLikeResultDto
            {
                Liked = liked,
                Likes = target == CommentLikeTarget.Comment ? comment.LikesCount : comment.ReplyLikesCount
            };
        }
    }
}