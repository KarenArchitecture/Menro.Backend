using Menro.Application.Comments.Services.Interfaces;
using Menro.Application.DTO;
using Menro.Application.Features.Comments.DTOs;
using Menro.Domain.Entities;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;

namespace Menro.Application.Comments.Services.Implementations
{
    public class CreateCommentService : ICreateCommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CreateCommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
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

            return (true, null);
        }
    }
}