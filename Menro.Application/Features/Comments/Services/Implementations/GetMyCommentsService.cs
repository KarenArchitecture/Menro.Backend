// Application/Comments/Services/Implementations/GetMyCommentsService.cs
using Menro.Application.Comments.Services.Interfaces;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Menro.Application.Features.Comments.DTOs;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;

namespace Menro.Application.Comments.Services.Implementations
{
    public class GetMyCommentsService : IGetMyCommentsService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IMediaStorageProvider _mediaStorage;

        public GetMyCommentsService(
            ICommentRepository commentRepository,
            IMediaStorageProvider mediaStorage)
        {
            _commentRepository = commentRepository;
            _mediaStorage = mediaStorage;
        }

        // Application/Comments/Services/Implementations/GetMyCommentsService.cs
        public async Task<List<MyCommentDto>> GetMyCommentsAsync(string userId)
        {
            var comments = await _commentRepository.GetByUserIdAsync(userId);

            var foodIds = comments.Select(c => c.FoodId).Distinct().ToList();
            var approvedCounts = await _commentRepository.GetApprovedCountsByFoodIdsAsync(foodIds);

            var result = new List<MyCommentDto>();

            foreach (var c in comments)
            {
                bool replyLiked = !string.IsNullOrEmpty(c.ReplyText) &&
                    await _commentRepository.GetLikeAsync(c.Id, userId, CommentLikeTarget.Reply) != null;

                bool liked = await _commentRepository.GetLikeAsync(c.Id, userId, CommentLikeTarget.Comment) != null;

                result.Add(new MyCommentDto
                {
                    Id = c.Id,
                    FoodId = c.FoodId,
                    FoodTitle = c.Food?.Name ?? string.Empty,
                    FoodImageUrl = string.IsNullOrWhiteSpace(c.Food?.ImageUrl)
                        ? null
                        : _mediaStorage.GetUrl(MediaCategory.RestaurantFoodImage, c.Food.ImageUrl, c.Food.Id.ToString(), MediaVariant.Thumbnail),
                    RestaurantName = c.Food?.Restaurant?.Name ?? string.Empty,
                    RestaurantSlug = c.Food?.Restaurant?.Slug ?? string.Empty,
                    ApprovedCommentsCount = approvedCounts.TryGetValue(c.FoodId, out var cnt) ? cnt : 0,
                    Status = c.Status.ToString().ToLower(),
                    Rating = c.Rating,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt,
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