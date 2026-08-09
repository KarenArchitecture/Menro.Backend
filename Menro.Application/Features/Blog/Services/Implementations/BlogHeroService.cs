using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Features.Blog.Services.Interfaces;
using Menro.Domain.Entities.Blog;
using Menro.Domain.Interfaces.Blog;

namespace Menro.Application.Features.Blog.Services.Implementations
{
    public class BlogHeroService : IBlogHeroService
    {
        private readonly IBlogHeroRepository _repository;

        public BlogHeroService(IBlogHeroRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Returns the hero config, or sensible defaults if it has never been
        /// saved yet (fresh install).
        /// </summary>
        public async Task<BlogHeroResponse> GetAsync(CancellationToken ct = default)
        {
            var hero = await _repository.GetAsync(ct);

            if (hero is null)
            {
                return new BlogHeroResponse(
                    Guid.Empty,
                    "بخون، بدون، با منرو",
                    "متفاوت باش",
                    "جستجو مقاله ...");
            }

            return ToResponse(hero);
        }

        public async Task<BlogHeroResponse> UpdateAsync(UpdateBlogHeroRequest request, CancellationToken ct = default)
        {
            var hero = new BlogHero
            {
                TitleLine = request.TitleLine.Trim(),
                Highlight = request.Highlight.Trim(),
                SearchPlaceholder = request.SearchPlaceholder.Trim()
            };

            var saved = await _repository.UpsertAsync(hero, ct);
            return ToResponse(saved);
        }

        private static BlogHeroResponse ToResponse(BlogHero hero) => new(
            hero.Id,
            hero.TitleLine,
            hero.Highlight,
            hero.SearchPlaceholder);
    }
}
