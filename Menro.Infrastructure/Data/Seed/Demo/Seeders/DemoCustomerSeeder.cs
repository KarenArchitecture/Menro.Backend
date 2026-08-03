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

    public DemoCustomerSeeder(MenroDbContext db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public int Order => SeedOrder.Customer;

    private const string Password = "Customer123!";

    // Index 0 (09121112233) stays fixed — DemoFavoriteFoodSeeder and other
    // seeders key specifically off this phone as "the" primary demo customer.
    private static readonly (string Phone, string Name)[] Customers =
    {
        ("09121112233", "مشتری نمونه"),
        ("09121112234", "سارا احمدی"),
        ("09121112235", "علی محمدی"),
        ("09121112236", "نگار کریمی"),
        ("09121112237", "امیر رضایی"),
    };

    public async Task SeedAsync()
    {
        foreach (var (rawPhone, name) in Customers)
        {
            var phone = ToE164(rawPhone);

            var customer = await _db.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phone);

            if (customer == null)
            {
                customer = new User
                {
                    UserName = rawPhone,
                    PhoneNumber = phone,
                    PhoneNumberConfirmed = true,
                    FullName = name
                };

                var result = await _userManager.CreateAsync(customer, Password);

                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));
            }

            var roles = await _userManager.GetRolesAsync(customer);

            if (!roles.Contains(SD.Role_Customer))
                await _userManager.AddToRoleAsync(customer, SD.Role_Customer);

            var hasPassword = await _userManager.HasPasswordAsync(customer);

            if (!hasPassword)
            {
                var result = await _userManager.AddPasswordAsync(customer, Password);

                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));
            }
        }

        Console.WriteLine($"[Seed] {Customers.Length} demo customers synced (password: {Password}).");
    }

    // 🔧 Local, self-contained conversion — deliberately NOT the shared
    // Utilities class. AuthController always normalizes login input to
    // "+98..." before looking the user up, so the PhoneNumber stored here
    // must already be in that form or login will never find a match.
    private static string ToE164(string rawPhone) => "+98" + rawPhone[1..];
}