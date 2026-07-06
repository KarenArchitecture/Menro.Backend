using Menro.Application.Comments.Services.Interfaces;
using Menro.Application.Common.Interfaces;
using Menro.Application.DTO;
using Menro.Application.Features.Comments.DTOs;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;

namespace Menro.Application.Comments.Services.Implementations
{
    public class GetFoodCommentsService : IGetFoodCommentsService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IFileUrlService _fileUrlService;

        public GetFoodCommentsService(
            ICommentRepository commentRepository,
            IFileUrlService fileUrlService)
        {
            _commentRepository = commentRepository;
            _fileUrlService = fileUrlService;
        }

        public async Task<List<CommentDto>> GetCommentsByFoodIdAsync(int foodId, string? currentUserId)
        {
            var comments = await _commentRepository.GetApprovedCommentsByFoodIdAsync(foodId);
            var result = new List<CommentDto>();

            foreach (var c in comments)
            {
                bool liked = false;
                bool replyLiked = false;

                if (!string.IsNullOrEmpty(currentUserId))
                {
                    liked = await _commentRepository.GetLikeAsync(c.Id, currentUserId, CommentLikeTarget.Comment) != null;

                    if (!string.IsNullOrEmpty(c.ReplyText))
                        replyLiked = await _commentRepository.GetLikeAsync(c.Id, currentUserId, CommentLikeTarget.Reply) != null;
                }

                result.Add(new CommentDto
                {
                    Id = c.Id,
                    FoodId = c.FoodId,
                    FoodTitle = c.Food?.Name ?? string.Empty,
                    FoodImageUrl = string.IsNullOrWhiteSpace(c.Food?.ImageUrl)
                        ? null
                        : _fileUrlService.BuildFoodImageUrl(c.Food.ImageUrl),
                    Rating = c.Rating,
                    Text = c.Text,
                    Likes = c.LikesCount,
                    Liked = liked,
                    Reply = string.IsNullOrEmpty(c.ReplyText)
                        ? null
                        : new CommentReplyDto
                        {
                            Text = c.ReplyText,
                            Date = c.ReplyDate ?? c.CreatedAt,
                            Likes = c.ReplyLikesCount,
                            Liked = replyLiked
                        }
                });
            }

            return result;
        }
    }
}