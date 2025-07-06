using Menro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Services.Interfaces
{
    public interface IUserService
    {
        public Task<User> GetByIdAsync(string id);
        public Task<User> GetByEmailAsync(string email);
        public Task<bool> RegisterUserAsync(string fullName, string email, string phoneNumber, string password);
        public Task<User?> LoginUserAsync(string email, string password);

    }
}
