using Menro.Application.Comments.Services.Interfaces;
using Menro.Application.DTO;
using Menro.Application.Features.Comments.DTOs;
using Menro.Domain.Enums;
using Menro.Domain.Interfaces;

namespace Menro.Application.Comments.Services.Implementations
{
    public class GetCommentsForAdminService : IGetCommentsForAdminService
    {
        private readonly ICommentRepository _commentRepository;

        public GetCommentsForAdminService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<List<CommentAdminDto>> GetCommentsAsync(string status)
        {
            var parsedStatus = status?.ToLower() switch
            {
                "approved" => CommentStatus.Approved,
                "rejected" => CommentStatus.Rejected,
                _ => CommentStatus.Pending
            };

            var comments = await _commentRepository.GetForAdminByStatusAsync(parsedStatus);

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