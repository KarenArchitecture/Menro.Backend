using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Comments.DTOs
{
    public class ApproveCommentDto
    {
        public int CommentId { get; set; }
        public string? ReplyText { get; set; }
    }
}
