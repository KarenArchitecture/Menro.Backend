using Menro.Application.Comments.Services.Interfaces;
using Menro.Application.Features.Comments.DTOs;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;
namespace Menro.Application.Comments.Services.Implementations
{
    public class GetCommentsForOwnerService : IGetCommentsForOwnerService
    {
        private readonly ICommentRepository _commentRepository;
        public GetCommentsForOwnerService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }
        public async Task<List<CommentAdminDto>> GetCommentsAsync(int restaurantId, string status)
        {
            var parsedStatus = status?.ToLower() switch
            {
                "approved" => CommentStatus.Approved,
                "rejected" => CommentStatus.Rejected,
                _ => CommentStatus.Pending
            };
            var comments = await _commentRepository.GetForRestaurantByStatusAsync(restaurantId, parsedStatus);
            return comments.Select(c => new CommentAdminDto
            {
                Id = c.Id,
                Code = $"CMT-{c.Id}",
                Status = parsedStatus.ToString().ToLower(),
                Title = c.Food?.Name ?? "نامشخص",
                UserName = c.User?.FullName ?? "کاربر مهمان",
                Rating = c.Rating,
                CommentText = c.Text,
                Date = c.CreatedAt.ToString("yyyy/MM/dd"),
                Reply = c.ReplyText,
                RejectReason = c.RejectReason
            }).ToList();
        }
    }
}