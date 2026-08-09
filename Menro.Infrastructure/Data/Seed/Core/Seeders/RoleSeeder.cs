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
        SD.Role_Customer,
        // blog related roles
        SD.Role_Contributor,
        SD.Role_Author,
        SD.Role_Editor
    };

        foreach (var role in roles)
        {
            var exists = await _roleManager.RoleExistsAsync(role);

            if (!exists)
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(role));

                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));

                Console.WriteLine($"[Seed] Role created: {role}");
            }
        }
    }
}