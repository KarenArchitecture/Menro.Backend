using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menro.Domain.Contracts;

namespace Menro.Domain.Interfaces
{
    public interface ISearchRepository
    {
        Task<List<SearchHit>> SearchAsync(string term, int take);
    }
}
