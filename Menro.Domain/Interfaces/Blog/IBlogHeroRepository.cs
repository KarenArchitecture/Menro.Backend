using Menro.Domain.Entities.Blog;

namespace Menro.Domain.Interfaces.Blog
{
    public interface IBlogHeroRepository
    {
        Task<BlogHero?> GetAsync(
            CancellationToken ct = default);

        Task<BlogHero> UpsertAsync(
            BlogHero hero,
            CancellationToken ct = default);
    }
}