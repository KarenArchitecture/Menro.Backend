using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Comments.DTOs
{
    public class CreateCommentDto
    {
        public int FoodId { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
