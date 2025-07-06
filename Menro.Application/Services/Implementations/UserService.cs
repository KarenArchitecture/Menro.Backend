using Menro.Application.Services.Interfaces;
using Menro.Domain.Entities;
using Menro.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menro.Application.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;


        public UserService(IUnitOfWork uow, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager)
        {
            _uow = uow;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }
        public async Task<User> GetByIdAsync(string userId)
        {
            var user = await _uow.User.GetByIdAsync(userId);
            if (user is null)
                throw new InvalidOperationException("User not found");
            return user;
        }
        public async Task<User> GetByEmailAsync(string email)
        {
            var user = await _uow.User.GetByEmailAsync(email);
            if (user is null)
                throw new InvalidOperationException("User not found");
            return user;

        }
        public async Task<bool> RegisterUserAsync(string fullName, string email, string phoneNumber, string password)
        {
            if (email is not null)
            {
                if (await _userManager.FindByEmailAsync(email) is not null)
                    return false;
            }
            if (phoneNumber is not null)
            {
                if (await _uow.User.GetByPhoneNumberAsync(phoneNumber) is not null)
                    return false;

            }

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                UserName = email
            };

            var result = await _userManager.CreateAsync(user, password);
            return result.Succeeded;
        }

        public async Task<User?> LoginUserAsync(string email, string password)
        {
            throw new NotImplementedException();
        }
    }
}
