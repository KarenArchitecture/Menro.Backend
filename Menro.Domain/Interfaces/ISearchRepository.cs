using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menro.Domain.Contracts;
using Menro.Domain.Enums;

namespace Menro.Domain.Interfaces
{
    public interface ISearchRepository
    {
        Task<List<SearchHit>> SearchAsync(string term, int take);
        Task<(List<SearchHit> Items, bool HasMore)> SearchPagedAsync(
            string term, SearchHitType type, int skip, int take, int? globalCategoryId = null);
    }
}
