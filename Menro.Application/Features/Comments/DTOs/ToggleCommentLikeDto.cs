using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Comments.DTOs
{
    public class ToggleCommentLikeDto
    {
        public int CommentId { get; set; }
        public string Target { get; set; } = "comment"; // "comment" | "reply"
    }
}
