using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Comments.DTOs
{
    public class CommentReplyDto
    {
        public string Text { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int Likes { get; set; }
        public bool Liked { get; set; }
    }
}
