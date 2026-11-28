using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shifa.Application.Interfaces.Admin;
using Shifa.Application.Interfaces.Appointments;
using Shifa.Application.Interfaces.Authentication;
using Shifa.Application.Interfaces.Doctors;
using Shifa.Application.Interfaces.MedicalRecords;
using Shifa.Application.Interfaces.Permissions;
using Shifa.Application.Interfaces.Users;
using Shifa.Application.Services.Admin;
using Shifa.Application.Services.Appointments;
using Shifa.Application.Services.MedicalRecords;
using Shifa.Application.Services.Permissions;
using Shifa.Application.Services.Users;
using Shifa.Infrastructure.Authentication;
using Shifa.Infrastructure.Identity;
using Shifa.Infrastructure.Persistence;
using Shifa.Infrastructure.Repositories.Admin;
using Shifa.Infrastructure.Repositories.Appointments;
using Shifa.Infrastructure.Repositories.Doctors;
using Shifa.Infrastructure.Repositories.MedicalRecords;
using Shifa.Infrastructure.Repositories.Permissions;
using Shifa.Infrastructure.Repositories.Users;
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
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();

        return services;
    }
}