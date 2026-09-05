using Menro.Application.Features.Comments.DTOs;
namespace Menro.Application.Comments.Services.Interfaces
{
    public interface IApproveCommentService
    {
        Task<CommentActionResult> ApproveAsync(int restaurantId, ApproveCommentDto dto);
    }
}