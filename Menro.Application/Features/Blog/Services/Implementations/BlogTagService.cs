using Menro.Application.Features.Blog.DTOs;
using Menro.Domain.Entities.Blog;
using Menro.Domain.Interfaces.Blog;

namespace Menro.Application.Features.Blog.Services.Implementations
{
    public class BlogTagService : IBlogTagService
    {
        private readonly IBlogTagRepository _repository;

        public BlogTagService(IBlogTagRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<BlogTagResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var tagsWithCounts = await _repository.GetAllWithArticleCountsAsync(ct);
            return tagsWithCounts
                .Select(x => new BlogTagResponse(x.Tag.Id, x.Tag.Name, x.ArticleCount))
                .ToList();
        }

        /// <summary>Throws InvalidOperationException if the name is already taken.</summary>
        public async Task<BlogTagResponse> CreateAsync(CreateBlogTagRequest request, CancellationToken ct = default)
        {
            var name = request.Name.Trim();

            if (await _repository.ExistsByNameAsync(name, ct: ct))
                throw new InvalidOperationException("این برچسب از قبل وجود دارد.");

            var tag = new BlogTag
            {
                Id = Guid.NewGuid(),
                Name = name,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _repository.AddAsync(tag, ct);
            return new BlogTagResponse(tag.Id, tag.Name, ArticleCount: 0);
        }

        /// <summary>
        /// Returns null if the tag doesn't exist. Throws InvalidOperationException
        /// if the new name collides with a different tag.
        /// </summary>
        public async Task<BlogTagResponse?> UpdateAsync(
            Guid id, UpdateBlogTagRequest request, CancellationToken ct = default)
        {
            var tag = await _repository.GetByIdAsync(id, ct);
            if (tag is null) return null;

            var name = request.Name.Trim();
            if (await _repository.ExistsByNameAsync(name, excludingId: id, ct: ct))
                throw new InvalidOperationException("این برچسب از قبل وجود دارد.");

            tag.Name = name;
            await _repository.UpdateAsync(tag, ct);

            var articleCount = tag.PostTags?.Count ?? 0;
            return new BlogTagResponse(tag.Id, tag.Name, articleCount);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var tag = await _repository.GetByIdAsync(id, ct);
            if (tag is null) return false;

            await _repository.DeleteAsync(tag, ct);
            return true;
        }
    }
}
