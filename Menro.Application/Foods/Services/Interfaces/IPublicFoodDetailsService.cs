using Menro.Application.Foods.DTOs;

namespace Menro.Application.Foods.Services.Interfaces
{
    /// <summary>
    /// Provides detailed food data (variants + addons + ratings)
    /// for the public restaurant page modal.
    /// </summary>
    public interface IPublicFoodDetailsService
    {
        Task<PublicFoodDetailDto?> GetFoodDetailsAsync(int foodId);
    }
}
