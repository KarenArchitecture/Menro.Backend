using System.Threading.Tasks;
using Menro.Domain.Entities.Landing;

namespace Menro.Domain.Interfaces.Landing
{
    public interface ILandingGeneralRepository
    {
        /// <summary>
        /// Returns the single settings row, creating it with default values
        /// on first access if it somehow doesn't exist yet.
        /// </summary>
        Task<LandingGeneral> GetOrCreateAsync();

        Task UpdateAsync(LandingGeneral entity);
    }
}
