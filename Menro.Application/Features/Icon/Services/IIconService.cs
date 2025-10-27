using Menro.Application.Features.Icons.DTOs;
using Menro.Domain.Entities;

namespace Menro.Application.Features.Icons.Interfaces
{
    public interface IIconService
    {
        Task<List<GetIconDto>> GetAllAsync();
        Task<GetIconDto?> GetByIdAsync(int id);
        Task<bool> AddAsync(AddIconDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
