using Menro.Application.Features.SiteContent.DTOs;
using Menro.Application.Features.SiteContent.Services.Interfaces;
using Menro.Domain.Entities.SiteContent;
using Menro.Domain.Interfaces.SiteContent;

namespace Menro.Application.Features.SiteContent.Services.Implementations
{
    public class MenuItemService : IMenuItemService
    {
        private readonly IMenuItemRepository _repository;

        public MenuItemService(IMenuItemRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<MenuItemDto>> GetPublicMenuAsync(MenuLocation location)
        {
            var items = await _repository.GetByLocationAsync(location, includeInactive: false);
            return items.Select(Map).ToList();
        }

        public async Task<List<MenuItemDto>> GetAdminMenuAsync(MenuLocation location)
        {
            var items = await _repository.GetByLocationAsync(location, includeInactive: true);
            return items.Select(Map).ToList();
        }

        public async Task<List<MenuItemDto>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(Map).ToList();
        }

        public async Task<MenuItemDto> CreateAsync(CreateMenuItemDto dto)
        {
            var maxOrder = await _repository.GetMaxOrderAsync(dto.Location);

            var entity = new MenuItem
            {
                Id = Guid.NewGuid(),
                Location = dto.Location,
                Title = dto.Title,
                Url = dto.Url,
                IsActive = dto.IsActive,
                ParentId = dto.ParentId,
                Order = maxOrder + 1
            };

            await _repository.AddAsync(entity);
            return Map(entity);
        }

        public async Task<MenuItemDto> UpdateAsync(Guid id, UpdateMenuItemDto dto)
        {
            var entity = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"MenuItem با شناسه {id} پیدا نشد.");

            entity.Title = dto.Title;
            entity.Url = dto.Url;
            entity.IsActive = dto.IsActive;
            entity.ParentId = dto.ParentId;

            await _repository.UpdateAsync(entity);
            return Map(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"MenuItem با شناسه {id} پیدا نشد.");

            await _repository.RemoveAsync(entity);
        }

        public async Task ReorderAsync(MenuLocation location, ReorderMenuItemDto dto)
        {
            var items = await _repository.GetByLocationAsync(location, includeInactive: true);
            var itemsById = items.ToDictionary(x => x.Id);

            var toUpdate = new List<MenuItem>();
            for (int i = 0; i < dto.OrderedIds.Count; i++)
            {
                if (itemsById.TryGetValue(dto.OrderedIds[i], out var entity))
                {
                    entity.Order = i + 1;
                    toUpdate.Add(entity);
                }
            }

            await _repository.ReorderAsync(toUpdate);
        }

        private static MenuItemDto Map(MenuItem entity) => new()
        {
            Id = entity.Id,
            Location = entity.Location.ToString(),
            Title = entity.Title,
            Url = entity.Url,
            Order = entity.Order,
            IsActive = entity.IsActive,
            ParentId = entity.ParentId
        };
    }
}