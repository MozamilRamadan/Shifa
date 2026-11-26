using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shifa.Infrastructure.Identity;
using Shifa.Infrastructure.Persistence;
using Shifa.Infrastructure.Authentication;
using Shifa.Application.Interfaces.Authentication;
using Shifa.Application.Interfaces.MedicalRecords;
using Shifa.Application.Services.MedicalRecords;
using Shifa.Infrastructure.Repositories.MedicalRecords;
namespace Shifa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        // PostgreSQL
        services.AddDbContext<ShifaDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));

        // ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;

            options.User.RequireUniqueEmail = true;

            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(15);
        })
        .AddEntityFrameworkStores<ShifaDbContext>()
        .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(
         configuration.GetSection(JwtOptions.SectionName));


        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
        services.AddScoped<IMedicalRecordService, MedicalRecordService>();

        return services;
    }
}