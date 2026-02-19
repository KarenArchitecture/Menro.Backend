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
    }
}
