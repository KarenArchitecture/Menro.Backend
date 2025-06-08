using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Services.Interfaces
{
    public interface IJwtService
    {
        public string GenerateToken(Guid userId, string fullName, string email, List<string> roles);
    }
}
