using Menro.Application.DTO;
using Menro.Application.Features.Comments.DTOs;

namespace Menro.Application.Comments.Services.Interfaces
{
    public interface IToggleCommentLikeService
    {
        Task<ToggleLikeResultDto?> ToggleLikeAsync(string userId, ToggleCommentLikeDto dto);
    }
}
