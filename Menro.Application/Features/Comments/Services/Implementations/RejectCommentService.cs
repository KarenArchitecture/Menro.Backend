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

        public async Task<bool> RejectAsync(RejectCommentDto dto)
        {
            var comment = await _commentRepository.GetByIdAsync(dto.CommentId);
            if (comment == null) return false;

            comment.Status = CommentStatus.Rejected;
            comment.RejectReason = dto.Reason?.Trim();
            comment.ReplyText = null;
            comment.ReplyDate = null;

            await _commentRepository.SaveChangesAsync();
            return true;
        }
    }
}