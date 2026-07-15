using Menro.Application.Features.Landing.DTOs;
using Menro.Domain.Interfaces.Landing;
using Menro.Application.Features.Landing.Services.Interfaces;
using Menro.Domain.Entities.Landing;

namespace Menro.Application.Features.Landing.Services.Implementations
{
    public class LandingFaqService : ILandingFaqService
    {
        private const int QuestionMaxLength = 120;
        private const int AnswerMaxLength = 1200;

        private readonly ILandingFaqRepository _repository;

        public LandingFaqService(ILandingFaqRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<LandingFaqResponse>> GetAllAsync()
        {
            var faqs = await _repository.GetAllOrderedAsync();
            return faqs.Select(MapToResponse).ToList();
        }

        public async Task<LandingFaqResponse> CreateAsync(CreateLandingFaqRequest request)
        {
            Validate(request.Question, request.Answer);

            var entity = new LandingFaq
            {
                Id = Guid.NewGuid(),
                Question = request.Question.Trim(),
                Answer = request.Answer.Trim(),
                SortOrder = await _repository.GetNextSortOrderAsync(),
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            };

            await _repository.AddAsync(entity);
            return MapToResponse(entity);
        }

        public async Task<LandingFaqResponse> UpdateAsync(Guid id, UpdateLandingFaqRequest request)
        {
            Validate(request.Question, request.Answer);

            var entity = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"سوالی با شناسه {id} پیدا نشد.");

            entity.Question = request.Question.Trim();
            entity.Answer = request.Answer.Trim();
            entity.UpdatedAtUtc = DateTime.UtcNow;

            await _repository.UpdateAsync(entity);
            return MapToResponse(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"سوالی با شناسه {id} پیدا نشد.");

            await _repository.DeleteAsync(entity);
        }

        public async Task MoveAsync(Guid id, string direction)
        {
            var normalizedDirection = (direction ?? string.Empty).Trim().ToLowerInvariant();
            if (normalizedDirection != "up" && normalizedDirection != "down")
                throw new ArgumentException("جهت جابجایی باید up یا down باشد.", nameof(direction));

            var ordered = await _repository.GetAllOrderedAsync();
            var currentIndex = ordered.FindIndex(f => f.Id == id);
            if (currentIndex < 0)
                throw new KeyNotFoundException($"سوالی با شناسه {id} پیدا نشد.");

            var targetIndex = normalizedDirection == "up" ? currentIndex - 1 : currentIndex + 1;
            if (targetIndex < 0 || targetIndex >= ordered.Count)
                return; // already at the edge - nothing to do, same as the frontend's move()

            var current = ordered[currentIndex];
            var target = ordered[targetIndex];
            (current.SortOrder, target.SortOrder) = (target.SortOrder, current.SortOrder);

            await _repository.UpdateRangeAsync(current, target);
        }

        private static void Validate(string question, string answer)
        {
            var trimmedQuestion = question?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmedQuestion))
                throw new ArgumentException("متن سوال الزامی است.", nameof(question));
            if (trimmedQuestion.Length > QuestionMaxLength)
                throw new ArgumentException(
                    $"سوال نباید بیشتر از {QuestionMaxLength} کاراکتر باشد.", nameof(question));

            var trimmedAnswer = answer?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmedAnswer))
                throw new ArgumentException("متن پاسخ الزامی است.", nameof(answer));
            if (trimmedAnswer.Length > AnswerMaxLength)
                throw new ArgumentException(
                    $"پاسخ نباید بیشتر از {AnswerMaxLength} کاراکتر باشد.", nameof(answer));
        }

        private static LandingFaqResponse MapToResponse(LandingFaq entity) =>
            new(entity.Id, entity.Question, entity.Answer, entity.SortOrder);
    }
}
