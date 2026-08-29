using Menro.Application.Features.Comments.DTOs;
namespace Menro.Application.Comments.Services.Interfaces
{
    public interface IGetCommentsForOwnerService
    {
        Task<List<CommentAdminDto>> GetCommentsAsync(int restaurantId, string status);
    }
}