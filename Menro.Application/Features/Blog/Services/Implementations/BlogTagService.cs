using Menro.Application.Features.Blog.DTOs;
using Menro.Application.Helpers;
using Menro.Domain.Entities.Blog;
using Menro.Domain.Interfaces.Blog;
using Microsoft.IdentityModel.Logging;

namespace Menro.Application.Features.Blog.Services.Implementations
{
    public class BlogTagService : IBlogTagService
    {
        /// <summary>
        /// Max number of tags that can be marked Suggested at the same time -
        /// keeps the public sidebar block short. Mirrored in the admin UI counter.
        /// </summary>
        public const int MaxSuggestedTags = 10;

        private readonly IBlogTagRepository _repository;

        public BlogTagService(IBlogTagRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<BlogTagResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var tagsWithCounts = await _repository.GetAllWithArticleCountsAsync(ct);
            return tagsWithCounts
                .Select(x => new BlogTagResponse(x.Tag.Id, x.Tag.Name, x.Tag.Slug, x.ArticleCount, x.Tag.Suggested))
                .ToList();
        }

        public async Task<IReadOnlyList<BlogTagResponse>> GetSuggestedAsync(CancellationToken ct = default)
        {
            var tagsWithCounts = await _repository.GetSuggestedWithArticleCountsAsync(ct);
            return tagsWithCounts
                .Select(x => new BlogTagResponse(x.Tag.Id, x.Tag.Name, x.Tag.Slug, x.ArticleCount, x.Tag.Suggested))
                .ToList();
        }

        // NEW - used by PublicBlogTaxonomyController to resolve /blog/tag/{slug}.
        public async Task<BlogTagResponse?> GetBySlugAsync(string slug, CancellationToken ct = default)
        {
            var tag = await _repository.GetBySlugAsync(slug, ct);
            if (tag is null) return null;

            var articleCount = tag.PostTags?.Count ?? 0;
            return new BlogTagResponse(tag.Id, tag.Name, tag.Slug, articleCount, tag.Suggested);
        }

        public async Task<BlogTagResponse> CreateAsync(CreateBlogTagRequest request, CancellationToken ct = default)
        {
            var name = request.Name.Trim();

            if (await _repository.ExistsByNameAsync(name, ct: ct))
                throw new InvalidOperationException("این برچسب از قبل وجود دارد.");

            // enforce the same cap that ToggleSuggestedAsync enforces
            if (request.Suggested == true)
            {
                var suggestedCount = await _repository.CountSuggestedAsync(ct);
                if (suggestedCount >= MaxSuggestedTags)
                    throw new InvalidOperationException(
                        $"حداکثر تعداد برچسب‌های پیشنهادی ({MaxSuggestedTags} عدد) است. برای اضافه کردن این یکی، اول یکی از قبلی‌ها را بردارید.");
            }

            var tag = new BlogTag
            {
                Id = Guid.NewGuid(),
                Name = name,
                Slug = await GenerateUniqueSlugAsync(name, ct),
                Suggested = request.Suggested,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _repository.AddAsync(tag, ct);
            return new BlogTagResponse(tag.Id, tag.Name, tag.Slug, ArticleCount: 0, tag.Suggested);
        }

        public async Task<BlogTagResponse?> UpdateAsync(
            Guid id, UpdateBlogTagRequest request, CancellationToken ct = default)
        {
            var tag = await _repository.GetByIdAsync(id, ct);
            if (tag is null) return null;

            var name = request.Name.Trim();
            if (await _repository.ExistsByNameAsync(name, excludingId: id, ct: ct))
                throw new InvalidOperationException("این برچسب از قبل وجود دارد.");

            tag.Name = name;
            // Slug is intentionally left untouched here so existing
            // /blog/tag/{slug} links never break when the name is edited.
            tag.Suggested = request.Suggested;
            await _repository.UpdateAsync(tag, ct);

            var articleCount = tag.PostTags?.Count ?? 0;
            return new BlogTagResponse(tag.Id, tag.Name, tag.Slug, articleCount, tag.Suggested);
        }

        /// <summary>
        /// Flips Suggested (null treated as false) — mirrors post publish/unpublish toggle.
        /// Turning a tag ON is rejected once <see cref="MaxSuggestedTags"/> tags are already suggested.
        /// </summary>
        public async Task<BlogTagResponse?> ToggleSuggestedAsync(Guid id, CancellationToken ct = default)
        {
            var tag = await _repository.GetByIdAsync(id, ct);
            if (tag is null) return null;

            var turningOn = !(tag.Suggested ?? false);
            if (turningOn)
            {
                var suggestedCount = await _repository.CountSuggestedAsync(ct);
                if (suggestedCount >= MaxSuggestedTags)
                    throw new InvalidOperationException(
                        $"حداکثر تعداد برچسب‌های پیشنهادی ({MaxSuggestedTags} عدد) است. برای اضافه کردن این یکی، اول یکی از قبلی‌ها را بردارید.");
            }

            tag.Suggested = turningOn;
            await _repository.UpdateAsync(tag, ct);

            var articleCount = tag.PostTags?.Count ?? 0;
            return new BlogTagResponse(tag.Id, tag.Name, tag.Slug, articleCount, tag.Suggested);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var tag = await _repository.GetByIdAsync(id, ct);
            if (tag is null) return false;

            await _repository.DeleteAsync(tag, ct);
            return true;
        }

        /// <summary>
        /// Generates a slug from <paramref name="source"/> and appends
        /// "-2", "-3", etc. if it collides with an existing one.
        /// </summary>
        private async Task<string> GenerateUniqueSlugAsync(string source, CancellationToken ct)
        {
            var baseSlug = SlugHelper.GenerateSlug(source);
            var slug = baseSlug;
            var suffix = 2;

            while (await _repository.ExistsBySlugAsync(slug, ct))
            {
                slug = $"{baseSlug}-{suffix}";
                suffix++;
            }

            return slug;
        }
    }
}
