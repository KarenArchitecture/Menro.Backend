using Menro.Application.Features.Landing.DTOs;
using Menro.Domain.Interfaces.Landing;
using Menro.Application.Features.Landing.Services.Interfaces;
using Menro.Application.Common.Interfaces;
using Menro.Application.Common.Media;
using Microsoft.AspNetCore.Http;

namespace Menro.Application.Features.Landing.Services.Implementations
{
    public class LandingGeneralService : ILandingGeneralService
    {
        #region DI
        private readonly ILandingGeneralRepository _repository;
        private readonly IMediaStorageProvider _mediaStorage;

        public LandingGeneralService(
            ILandingGeneralRepository repository,
            IMediaStorageProvider mediaStorage)
        {
            _repository = repository;
            _mediaStorage = mediaStorage;
        }
        #endregion

        // < constants
        private const int HeroHighlightMaxLength = 60;
        private const int HeroTitleMaxLength = 60;
        private const int SpotlightTitleMaxLength = 60;
        // >

        public async Task<LandingGeneralResponse> GetAsync()
        {
            var entity = await _repository.GetOrCreateAsync();
            return MapToResponse(entity);
        }

        public async Task<LandingGeneralResponse> UpdateAsync(UpdateLandingGeneralRequest request)
        {
            Validate(request);

            var entity = await _repository.GetOrCreateAsync();

            // If the admin removed the image (saved with an empty file name)
            // without uploading a replacement, clean up the orphaned file.
            // Replacement uploads are already handled at upload time - see
            // UploadHeroImageAsync below - so we don't double-delete here.
            var isRemovingWithoutReplacement =
                !string.IsNullOrWhiteSpace(entity.HeroImageFileName) &&
                string.IsNullOrWhiteSpace(request.HeroImageFileName);

            if (isRemovingWithoutReplacement)
            {
                _mediaStorage.Delete(MediaCategory.LandingHeroImage, entity.HeroImageFileName!);
            }

            entity.HeroHighlight = request.HeroHighlight.Trim();
            entity.HeroTitle = request.HeroTitle.Trim();
            entity.SpotlightTitle = request.SpotlightTitle.Trim();
            entity.HeroImageFileName = string.IsNullOrWhiteSpace(request.HeroImageFileName)
                ? null
                : request.HeroImageFileName;
            entity.UpdatedAtUtc = DateTime.UtcNow;

            await _repository.UpdateAsync(entity);
            return MapToResponse(entity);
        }

        public async Task<UploadLandingHeroImageResponse> UploadHeroImageAsync(IFormFile file, string? oldFileName)
        {
            if (file is null || file.Length == 0)
                throw new ArgumentException("فایلی برای آپلود ارسال نشده است.", nameof(file));

            // SaveAsync deletes oldFileName as part of the same call -
            // same convention as blog post covers.
            var result = await _mediaStorage.SaveAsync(
                MediaCategory.LandingHeroImage, file, oldFileName: oldFileName);

            return new UploadLandingHeroImageResponse(result.FileName, result.Url);
        }

        private static void Validate(UpdateLandingGeneralRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.HeroHighlight))
                throw new ArgumentException("متن هایلایت هیرو الزامی است.", nameof(request.HeroHighlight));
            if (request.HeroHighlight.Trim().Length > HeroHighlightMaxLength)
                throw new ArgumentException(
                    $"متن هایلایت هیرو نباید بیشتر از {HeroHighlightMaxLength} کاراکتر باشد.",
                    nameof(request.HeroHighlight));

            if (string.IsNullOrWhiteSpace(request.HeroTitle))
                throw new ArgumentException("ادامه متن هیرو الزامی است.", nameof(request.HeroTitle));
            if (request.HeroTitle.Trim().Length > HeroTitleMaxLength)
                throw new ArgumentException(
                    $"ادامه متن هیرو نباید بیشتر از {HeroTitleMaxLength} کاراکتر باشد.",
                    nameof(request.HeroTitle));

            if (string.IsNullOrWhiteSpace(request.SpotlightTitle))
                throw new ArgumentException(
                    "عنوان بخش «با منرو تو چشم باش» الزامی است.",
                    nameof(request.SpotlightTitle));
            if (request.SpotlightTitle.Trim().Length > SpotlightTitleMaxLength)
                throw new ArgumentException(
                    $"عنوان این بخش نباید بیشتر از {SpotlightTitleMaxLength} کاراکتر باشد.",
                    nameof(request.SpotlightTitle));
        }

        private LandingGeneralResponse MapToResponse(Domain.Entities.Landing.LandingGeneral entity) =>
            new(
                entity.Id,
                string.IsNullOrWhiteSpace(entity.HeroImageFileName)
                    ? null
                    : _mediaStorage.GetUrl(MediaCategory.LandingHeroImage, entity.HeroImageFileName),
                entity.HeroHighlight,
                entity.HeroTitle,
                entity.SpotlightTitle);
    }
}