using EcommerceStore.Constants;
using EcommerceStore.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceStore.Data;

public static class DbInitializer
{
    public static async Task SeedCatalogAsync(ApplicationDbContext db)
    {
        if (await db.Categories.AnyAsync()) return;

        var categories = new List<Category>
        {
            new() { Name = "Club Kits",       Description = "Official and replica kits from top clubs worldwide." },
            new() { Name = "National Teams",  Description = "International national team jerseys." },
            new() { Name = "Retro Classics",  Description = "Vintage and throwback football shirts." },
            new() { Name = "Limited Edition", Description = "Exclusive, limited-run performance wear." },
        };

        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        var clubKits      = categories[0];
        var nationalTeams = categories[1];
        var retro         = categories[2];
        var limited       = categories[3];

        var products = new List<Product>
        {
            new() { Name = "Neon Strike V2",    Description = "High-visibility training jersey with micro-ventilation.", Price = 129.00m, Stock = 50, CategoryId = limited.Id,       IsActive = true, ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuBjVZprmP2dMJoZyoqdfrh6-ageADxNuNJQShmAqnh26YRQ18f4beVkUMVIRSyJFyfCszta_W2OqzCTO5KXFzhvhDxhsA2JwdKVZQ31MdmlckdFncmD3UbDXVB-YbGhGNeS1YvzbYgZwQVWhE8yW08dqU3m1Tc8fg95_qpkyLqeeh8LqZR9H9bEKNVkiztetswlanObE1zXSjjIq3X74tVdaR9FnW4roZc4VN6enPKQi1s_hr5vC7u78kRZOSphi5-bo1PhPJo4-fM" },
            new() { Name = "Cyber City Kit",    Description = "Urban-inspired jersey inspired by city lights.", Price = 110.00m, Stock = 75, CategoryId = clubKits.Id,      IsActive = true, ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuBiCM5-5VPmXi7xnd7pgB70Yqzb6TijBZIINhunQsPDCCnwIkKqHMkQDj2j8Ie_gliKGMUI9OeubepYDM69dThHyU1QZpu7O2hgswQjHkkx4FoGmp4unE3N8Qdnl7zfQsCM_Ivz30XcqZpEpXxurSDVm8RploIXSxZ9wnE_4J44eR1bPNu9itNruBjwTc3ADveIfyom6esX-gcp-UVmVkY-j3r94QJmHGhbeQbdXu1MfvVPqLnuof65AyAK-qvTf18pEgIRjvcvths" },
            new() { Name = "Phantom Stealth",   Description = "Matte black performance jersey with adaptive thermal control.", Price = 145.00m, Stock = 30, CategoryId = limited.Id,       IsActive = true, ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuAZRK54ss8KCFvktiTLFkVT2pBLyHmLijMuwT1KDpSETa8sZQrhkKGseBYzrO0IqzPY3N6KiK7JnOkb-MyuqkUCh58i-hWTTQPlDcBjeZKR8CVjluIoBtFIDnPBfeFu8kdHEhKb5rVMSsIc4Y_uSbaiIsevzzbCts7WqI-3WxWg_Ircn1l3Awu4dqYtqFbYiYVAsMKdKy_V2rAFefiqGjS-MirHHbO0j8L0vgc6OcLwBcWbZsPnq79z3j9aXFMd4jWvLj-A1zxFCzo" },
            new() { Name = "Heritage FC Strip", Description = "Classic 90s-inspired club strip with modern fabric.", Price = 89.00m,  Stock = 100, CategoryId = retro.Id,         IsActive = true, ImageUrl = "" },
            new() { Name = "Galactico Away",    Description = "Official-style away kit with ultra-light construction.", Price = 119.00m, Stock = 60, CategoryId = nationalTeams.Id, IsActive = true, ImageUrl = "" },
            new() { Name = "Pulse Home Kit",    Description = "Home kit with embedded biometric sensor zones.", Price = 135.00m, Stock = 45, CategoryId = clubKits.Id,      IsActive = true, ImageUrl = "" },
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }

    public static async Task SeedIdentityAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await EnsureRoleAsync(roleManager, Roles.Admin);
        await EnsureRoleAsync(roleManager, Roles.Customer);

        var adminSection = configuration.GetSection("SeedAdminUser");
        var email = adminSection["Email"];
        var password = adminSection["Password"];
        var fullName = adminSection["FullName"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName ?? "Store Admin",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create seeded admin user: {errors}");
            }

            await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            await userManager.AddToRoleAsync(adminUser, Roles.Customer);
            return;
        }

        if (!await userManager.IsInRoleAsync(existingUser, Roles.Admin))
        {
            await userManager.AddToRoleAsync(existingUser, Roles.Admin);
        }

        if (!await userManager.IsInRoleAsync(existingUser, Roles.Customer))
        {
            await userManager.AddToRoleAsync(existingUser, Roles.Customer);
        }
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
