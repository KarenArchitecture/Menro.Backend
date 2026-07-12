using Menro.Domain.Entities.Blog;

namespace Menro.Domain.Interfaces.Blog
{
    public interface IBlogPostRepository
    {
        /// <summary>
        /// Returns posts filtered by search text and optionally by display category.
        /// Implementations should include the Category navigation so CategoryTitle
        /// can be populated on the response DTO.
        /// </summary>
        Task<IReadOnlyList<BlogPost>> GetAllAsync(
            string? search, Guid? categoryId, CancellationToken ct = default);

        /// <summary>Should include the Category navigation.</summary>
        Task<BlogPost?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task AddAsync(BlogPost post, CancellationToken ct = default);

        Task UpdateAsync(BlogPost post, CancellationToken ct = default);

        Task DeleteAsync(BlogPost post, CancellationToken ct = default);
    }
}
