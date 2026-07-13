using Menro.Application.Comments.Services.Interfaces;
using Menro.Application.Common.Interfaces;
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

        public async Task<FoodCommentsResponseDto?> GetCommentsByFoodIdAsync(int foodId, string? currentUserId)
        {
            var foodSummary = await _commentRepository.GetFoodSummaryAsync(foodId);
            if (foodSummary == null) return null;

            var comments = await _commentRepository.GetApprovedCommentsByFoodIdAsync(foodId);
            var approvedCount = await _commentRepository.GetApprovedCountByFoodIdAsync(foodId);

            bool hasUserCommented = !string.IsNullOrEmpty(currentUserId) &&
                await _commentRepository.UserAlreadyCommentedAsync(foodId, currentUserId);

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
                    UserName = c.User?.FullName ?? "کاربر مهمان",
                    UserAvatarUrl = string.IsNullOrWhiteSpace(c.User?.ProfileImage)
                        ? null
                        : _fileUrlService.BuildProfileImageUrl(c.User.ProfileImage),
                    CreatedAt = c.CreatedAt,
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

            return new FoodCommentsResponseDto
            {
                FoodId = foodId,
                FoodTitle = foodSummary.Title,
                FoodImageUrl = string.IsNullOrWhiteSpace(foodSummary.ImageUrl)
                    ? null
                    : _fileUrlService.BuildFoodImageUrl(foodSummary.ImageUrl),
                RestaurantName = foodSummary.RestaurantName,
                RestaurantSlug = foodSummary.RestaurantSlug,
                ApprovedCommentsCount = approvedCount,
                HasUserCommented = hasUserCommented,
                Comments = result
            };
        }
    }
}