using Menro.Application.Common.SD;
using Menro.Domain.Entities;
using Menro.Infrastructure.Data;
using Menro.Infrastructure.Data.Seed.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Menro.Infrastructure.Data.Seed.Demo.Seeders;

public class DemoCustomerSeeder : IDataSeeder
{
    private readonly MenroDbContext _db;
    private readonly UserManager<User> _userManager;

    public DemoCustomerSeeder(
        MenroDbContext db,
        UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public int Order => SeedOrder.Customer;

    public async Task SeedAsync()
    {
        const string demoPhone = "09121112233";
        const string password = "Customer123!";

        var customer = await _db.Users
            .FirstOrDefaultAsync(x => x.PhoneNumber == demoPhone);

        if (customer == null)
        {
            customer = new User
            {
                UserName = demoPhone,
                PhoneNumber = demoPhone,
                PhoneNumberConfirmed = true,
                FullName = "مشتری نمونه"
            };

            var result = await _userManager.CreateAsync(customer, password);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));
        }

        var roles = await _userManager.GetRolesAsync(customer);

        if (!roles.Contains(SD.Role_Customer))
            await _userManager.AddToRoleAsync(customer, SD.Role_Customer);

        var hasPassword = await _userManager.HasPasswordAsync(customer);

        if (!hasPassword)
        {
            var result = await _userManager.AddPasswordAsync(customer, password);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));
        }

        Console.WriteLine("[Seed] Demo customer synced.");
    }
}