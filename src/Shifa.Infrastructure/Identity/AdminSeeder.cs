using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shifa.Infrastructure.Identity;

public static class AdminSeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<int>>>();

        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];
        var firstName = configuration["AdminSeed:FirstName"];
        var lastName = configuration["AdminSeed:LastName"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName))
        {
            throw new InvalidOperationException(
                "Admin seed configuration is incomplete.");
        }

        const string adminRole = "Admin";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            var roleResult = await roleManager.CreateAsync(
                new IdentityRole<int>(adminRole));

            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    string.Join("; ", roleResult.Errors.Select(e => e.Description)));
        }

        var admin = await userManager.FindByEmailAsync(email);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(admin, password);

            if (!createResult.Succeeded)
                throw new InvalidOperationException(
                    string.Join("; ", createResult.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(admin, adminRole))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, adminRole);

            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    string.Join("; ", roleResult.Errors.Select(e => e.Description)));
        }
    }
}