using Menro.Application.Features.Icons.DTOs;
using Menro.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Icons.Interfaces
{
    public interface IIconService
    {
        Task<List<GetIconDto>> GetAllAsync();
        Task<GetIconDto?> GetByIdAsync(int id);
        Task<bool> AddAsync(string label, IFormFile icon);
        Task<bool> DeleteAsync(int id);
    }
}
