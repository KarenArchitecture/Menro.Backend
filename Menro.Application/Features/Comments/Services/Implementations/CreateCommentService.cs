using Menro.Application.Comments.Services.Interfaces;
using Menro.Application.DTO;
using Menro.Application.Features.Comments.DTOs;
using Menro.Application.FoodRatings.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;

namespace Menro.Application.Comments.Services.Implementations
{
    public class CreateCommentService : ICreateCommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IUpsertFoodRatingService _upsertFoodRatingService;

        public CreateCommentService(
            ICommentRepository commentRepository,
            IUpsertFoodRatingService upsertFoodRatingService)
        {
            _commentRepository = commentRepository;
            _upsertFoodRatingService = upsertFoodRatingService;
        }

        public async Task<(bool Success, string? Error)> CreateCommentAsync(string userId, CreateCommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return (false, "کاربر شناسایی نشد.");

            if (string.IsNullOrWhiteSpace(dto.Text))
                return (false, "متن نظر الزامی است.");

            if (dto.Rating < 1 || dto.Rating > 5)
                return (false, "امتیاز باید بین ۱ تا ۵ باشد.");

            var alreadyCommented = await _commentRepository.UserAlreadyCommentedAsync(dto.FoodId, userId);
            if (alreadyCommented)
                return (false, "شما قبلاً برای این غذا نظر ثبت کرده‌اید.");

            var comment = new Comment
            {
                FoodId = dto.FoodId,
                UserId = userId,
                Rating = dto.Rating,
                Text = dto.Text.Trim(),
                Status = CommentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepository.AddCommentAsync(comment);
            await _commentRepository.SaveChangesAsync();

            // Keep Food's aggregate rating in sync, independent of comment approval status
            await _upsertFoodRatingService.UpsertAsync(userId, dto.FoodId, dto.Rating);

            return (true, null);
        }
    }
}