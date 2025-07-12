using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Menro.Domain.Entities
{
    public class User : IdentityUser
    {
        [Display(Name = "نام کامل")]
        [MaxLength(50)]
        [Required(ErrorMessage = "نام کامل الزامی است")]
        public required string FullName { get; set; }

        [Display(Name = "آدرس عکس")]
        public string? ProfileImageUrl { get; set; } = string.Empty;
        // تاریخ عضویت
        public Restaurant? Restaurant { get; set; } = null;
    }
}
