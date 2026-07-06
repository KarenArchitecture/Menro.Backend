using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Comments.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        public int FoodId { get; set; }
        public string FoodTitle { get; set; } = string.Empty;
        public string? FoodImageUrl { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Likes { get; set; }
        public bool Liked { get; set; }
        public CommentReplyDto? Reply { get; set; }
    }
}
