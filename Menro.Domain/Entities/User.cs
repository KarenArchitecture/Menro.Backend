using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Menro.Domain.Entities
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; } = string.Empty;

        public Restaurant? Restaurant { get; set; } = null;
    }
}
