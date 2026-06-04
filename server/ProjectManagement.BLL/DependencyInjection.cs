using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectManagement.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBLL(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<LoginRequestDtoValidator>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IReleaseService, ReleaseService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJiraSettingsService, JiraSettingsService>();
        services.AddScoped<IJiraService, JiraService>();

        return services;
    }
}
