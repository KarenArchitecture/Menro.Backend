using Menro.Application.Features.Comments.DTOs;
namespace Menro.Application.Comments.Services.Interfaces
{
    public interface IRejectCommentService
    {
        Task<CommentActionResult> RejectAsync(int restaurantId, RejectCommentDto dto);
    }
}