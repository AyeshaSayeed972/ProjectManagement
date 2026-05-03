using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectManagement.DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddDAL(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<User, IdentityRole<int>>(options =>
        {
            options.Password.RequireDigit           = false;
            options.Password.RequiredLength         = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase       = false;
        })
        .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReleaseRepository, ReleaseRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IJiraSettingsRepository, JiraSettingsRepository>();

        return services;
    }
}
