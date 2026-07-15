using Menro.Application.Features.Comments.DTOs;

namespace Menro.Application.Comments.Services.Interfaces
{
    public interface IGetFoodCommentsService
    {
        Task<List<CommentDto>> GetCommentsByFoodIdAsync(int foodId, string? currentUserId);
    }
}
