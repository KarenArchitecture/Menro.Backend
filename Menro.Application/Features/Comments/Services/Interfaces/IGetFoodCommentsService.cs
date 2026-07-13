// Application/Comments/Services/Interfaces/IGetFoodCommentsService.cs
using Menro.Application.Features.Comments.DTOs;

namespace Menro.Application.Comments.Services.Interfaces
{
    public interface IGetFoodCommentsService
    {
        Task<FoodCommentsResponseDto?> GetCommentsByFoodIdAsync(int foodId, string? currentUserId);
    }
}