using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Comments.DTOs
{
    public class RejectCommentDto
    {
        public int CommentId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
