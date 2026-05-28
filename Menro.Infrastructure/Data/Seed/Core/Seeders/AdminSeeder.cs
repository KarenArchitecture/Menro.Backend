using Menro.Application.Common.SD;
using Menro.Domain.Entities;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Seed.Core.Seeders;

public class AdminSeeder : IDataSeeder
{
    private readonly UserManager<User> _userManager;

    public AdminSeeder(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    public int Order => SeedOrder.Admin;
    public async Task SeedAsync()
    {
        const string adminEmail = "MenroAdmin@gmail.com";

        var existingAdmin = await _userManager.Users
            .FirstOrDefaultAsync(x => x.Email == adminEmail);

        if (existingAdmin != null)
        {
            Console.WriteLine("[Seed] Admin already exists.");
            return;
        }

        var admin = new User
        {
            UserName = "MenroAdmin_1",
            Email = adminEmail,
            FullName = "مدیر",
            PhoneNumber = "+989486813486",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true
        };

        var result = await _userManager.CreateAsync(
            admin,
            "@Admin123456");

        if (!result.Succeeded)
        {
            var errors = string.Join(", ",
                result.Errors.Select(x => x.Description));

            throw new Exception(
                $"Failed to create admin user. Errors: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(
            admin,
            SD.Role_Admin);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ",
                roleResult.Errors.Select(x => x.Description));

            throw new Exception(
                $"Failed to assign admin role. Errors: {errors}");
        }

        Console.WriteLine("[Seed] Admin user created.");
    }
}