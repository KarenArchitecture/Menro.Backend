using Menro.Application.DTO;
using Menro.Application.Features.Comments.DTOs;

namespace Menro.Application.Comments.Services.Interfaces
{
    public interface IRejectCommentService
    {
        Task<bool> RejectAsync(RejectCommentDto dto);
    }
}