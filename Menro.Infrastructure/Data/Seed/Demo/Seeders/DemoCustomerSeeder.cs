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
        var demoPhone = "09121112233";

        var existingCustomer = await _db.Users
            .FirstOrDefaultAsync(
                x => x.PhoneNumber == demoPhone);

        if (existingCustomer != null)
        {
            Console.WriteLine(
                "[Seed] Demo customer already seeded.");

            return;
        }

        var demoCustomer = new User
        {
            UserName = demoPhone,

            PhoneNumber = demoPhone,

            FullName = "مشتری نمونه"
        };

        var createResult = await _userManager
            .CreateAsync(
                demoCustomer,
                "Customer123!");

        if (!createResult.Succeeded)
        {
            var errors = string.Join(
                ", ",
                createResult.Errors.Select(
                    x => x.Description));

            throw new Exception(errors);
        }

        await _userManager.AddToRoleAsync(
            demoCustomer,
            SD.Role_Customer);

        Console.WriteLine(
            "[Seed] Demo customer seeded.");
    }
}