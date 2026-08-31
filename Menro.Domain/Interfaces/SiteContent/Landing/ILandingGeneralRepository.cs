using System.Threading.Tasks;
using Menro.Domain.Entities.SiteContent;

namespace Menro.Domain.Interfaces.SiteContent
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
