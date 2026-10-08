using Menro.Application.Features.Search.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Search.Services.Interfaces
{
    public interface IPublicSearchService
    {
        Task<SearchResponseDto> SearchAsync(string term, int take = 15);

        Task<PagedResultDto<SearchItemDto>> SearchPagedAsync(
            string term, SearchItemType type, int skip = 0, int take = 12, int? categoryId = null);
    }
}
