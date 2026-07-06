using Menro.Application.DTO;
using Menro.Application.Features.Comments.DTOs;

namespace Menro.Application.Comments.Services.Interfaces
{
    public interface ICreateCommentService
    {
        Task<(bool Success, string? Error)> CreateCommentAsync(string userId, CreateCommentDto dto);
    }
}