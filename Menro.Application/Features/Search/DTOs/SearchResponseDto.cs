using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Menro.Application.Features.Search.DTOs
{
    public class SearchResponseDto
    {
        public string Term { get; set; } = "";
        public List<SearchItemDto> Items { get; set; } = new();
    }
}
