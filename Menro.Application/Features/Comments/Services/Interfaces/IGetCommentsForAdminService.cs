using Menro.Application.Features.Comments.DTOs;

namespace Menro.Application.Comments.Services.Interfaces
{
    public interface IGetCommentsForAdminService
    {
        Task<List<CommentAdminDto>> GetCommentsAsync(string status);
    }
}
