using Menro.Application.Features.Landing.DTOs;
using Menro.Domain.Interfaces.Landing;
using Menro.Application.Features.Landing.Services.Interfaces;
using Menro.Domain.Entities.Landing;

namespace Menro.Application.Features.Landing.Services.Implementations
{
    public class LandingReasonService : ILandingReasonService
    {
        private const int TitleMaxLength = 30;
        private const int DescriptionMaxLength = 150;
        private const string DefaultColorHex = "#F59E0B";

        // "چرا منرو؟" is a fixed 4-card layout on the landing page (see
        // LandingManagementSection.jsx), so more than 4 reasons has nowhere
        // to render. Same cap-on-create pattern as MaxSuggestedTags in
        // BlogTagService.
        private const int MaxReasonsCount = 4;

        private readonly ILandingReasonRepository _repository;

        public LandingReasonService(ILandingReasonRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<LandingReasonResponse>> GetAllAsync()
        {
            var reasons = await _repository.GetAllOrderedAsync();
            return reasons.Select(MapToResponse).ToList();
        }

        public async Task<LandingReasonResponse> CreateAsync(CreateLandingReasonRequest request)
        {
            Validate(request.Icon, request.Title, request.Description);

            var currentCount = (await _repository.GetAllOrderedAsync()).Count;
            if (currentCount >= MaxReasonsCount)
                throw new InvalidOperationException(
                    $"حداکثر {MaxReasonsCount} دلیل قابل ثبت است. برای افزودن مورد جدید، ابتدا یکی از موارد فعلی را حذف کنید.");

            var entity = new LandingReason
            {
                Id = Guid.NewGuid(),
                Icon = request.Icon.Trim(),
                ColorHex = NormalizeColor(request.ColorHex),
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                SortOrder = await _repository.GetNextSortOrderAsync(),
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            };

            await _repository.AddAsync(entity);
            return MapToResponse(entity);
        }

        public async Task<LandingReasonResponse> UpdateAsync(Guid id, UpdateLandingReasonRequest request)
        {
            Validate(request.Icon, request.Title, request.Description);

            var entity = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"دلیلی با شناسه {id} پیدا نشد.");

            entity.Icon = request.Icon.Trim();
            entity.ColorHex = NormalizeColor(request.ColorHex);
            entity.Title = request.Title.Trim();
            entity.Description = request.Description.Trim();
            entity.UpdatedAtUtc = DateTime.UtcNow;

            await _repository.UpdateAsync(entity);
            return MapToResponse(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"دلیلی با شناسه {id} پیدا نشد.");

            await _repository.DeleteAsync(entity);
        }

        public async Task MoveAsync(Guid id, string direction)
        {
            var normalizedDirection = (direction ?? string.Empty).Trim().ToLowerInvariant();
            if (normalizedDirection != "up" && normalizedDirection != "down")
                throw new ArgumentException("جهت جابجایی باید up یا down باشد.", nameof(direction));

            var ordered = await _repository.GetAllOrderedAsync();
            var currentIndex = ordered.FindIndex(r => r.Id == id);
            if (currentIndex < 0)
                throw new KeyNotFoundException($"دلیلی با شناسه {id} پیدا نشد.");

            var targetIndex = normalizedDirection == "up" ? currentIndex - 1 : currentIndex + 1;
            if (targetIndex < 0 || targetIndex >= ordered.Count)
                return; // already at the edge - nothing to do, same as the frontend's move()

            var current = ordered[currentIndex];
            var target = ordered[targetIndex];
            (current.SortOrder, target.SortOrder) = (target.SortOrder, current.SortOrder);

            await _repository.UpdateRangeAsync(current, target);
        }

        private static void Validate(string icon, string title, string description)
        {
            if (string.IsNullOrWhiteSpace(icon))
                throw new ArgumentException("آیکون الزامی است.", nameof(icon));

            var trimmedTitle = title?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmedTitle))
                throw new ArgumentException("عنوان الزامی است.", nameof(title));
            if (trimmedTitle.Length > TitleMaxLength)
                throw new ArgumentException(
                    $"عنوان نباید بیشتر از {TitleMaxLength} کاراکتر باشد.", nameof(title));

            var trimmedDescription = description?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmedDescription))
                throw new ArgumentException("متن الزامی است.", nameof(description));
            if (trimmedDescription.Length > DescriptionMaxLength)
                throw new ArgumentException(
                    $"متن نباید بیشتر از {DescriptionMaxLength} کاراکتر باشد.", nameof(description));
        }

        private static string NormalizeColor(string? colorHex) =>
            string.IsNullOrWhiteSpace(colorHex) ? DefaultColorHex : colorHex.Trim();

        private static LandingReasonResponse MapToResponse(LandingReason entity) =>
            new(entity.Id, entity.Icon, entity.ColorHex, entity.Title, entity.Description, entity.SortOrder);
    }
}
