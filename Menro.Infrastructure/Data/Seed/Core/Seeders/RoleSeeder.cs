using Menro.Application.Common.SD;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.AspNetCore.Identity;

namespace Menro.Infrastructure.Data.Seed.Core.Seeders;

public class RoleSeeder : IDataSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleSeeder(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }
    public int Order => SeedOrder.Role;
    public async Task SeedAsync()
    {
        var roles = new[]
        {
            SD.Role_Admin,
            SD.Role_Owner,
            SD.Role_Customer
        };

        foreach (var role in roles)
        {
            var exists = await _roleManager.RoleExistsAsync(role);

            if (exists)
                continue;

            var result = await _roleManager.CreateAsync(
                new IdentityRole(role));

            if (!result.Succeeded)
            {
                var errors = string.Join(", ",
                    result.Errors.Select(x => x.Description));

                throw new Exception(
                    $"Failed to create role '{role}'. Errors: {errors}");
            }

            Console.WriteLine($"[Seed] Role created: {role}");
        }
    }
}