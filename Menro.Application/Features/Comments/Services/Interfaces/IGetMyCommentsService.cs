// Application/Comments/Services/Interfaces/IGetMyCommentsService.cs
using Menro.Application.Features.Comments.DTOs;

namespace Menro.Application.Comments.Services.Interfaces
{
    public interface IGetMyCommentsService
    {
        Task<List<MyCommentDto>> GetMyCommentsAsync(string userId);
    }
}