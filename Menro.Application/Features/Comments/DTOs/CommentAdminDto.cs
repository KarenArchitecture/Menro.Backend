using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Features.Comments.DTOs
{
    public class CommentAdminDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string? Reply { get; set; }
        public string? RejectReason { get; set; }
    }
}
