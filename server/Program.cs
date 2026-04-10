using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProjectManagement.Data;
using ProjectManagement.Entities;
using ProjectManagement.Middleware;
using ProjectManagement.Repositories;
using ProjectManagement.Repositories.Interfaces;
using ProjectManagement.Services;
using ProjectManagement.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var isDev   = builder.Environment.IsDevelopment();

// Controllers + JSON enum string support
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

// Swagger with cookie auth support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("CookieAuth", new OpenApiSecurityScheme
    {
        Name = "auth_session",
        Type = SecuritySchemeType.ApiKey,
        In   = ParameterLocation.Cookie,
        Description = "Identity session cookie (set automatically on login)"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "CookieAuth" }
            },
            Array.Empty<string>()
        }
    });
});

// EF Core – SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Core Identity (includes SignInManager)
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit           = false;
    options.Password.RequiredLength         = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase       = false;
})
.AddEntityFrameworkStores<AppDbContext>();

// Application cookie — Identity cookie session
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name        = "auth_session";
    options.Cookie.HttpOnly    = true;
    options.Cookie.SameSite    = isDev ? SameSiteMode.Lax : SameSiteMode.None;
    options.Cookie.SecurePolicy = isDev ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.ExpireTimeSpan     = TimeSpan.FromDays(7);
    options.SlidingExpiration  = true;

    // Return 401/403 JSON for API instead of redirecting to login page
    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = 401;
        return System.Threading.Tasks.Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = 403;
        return System.Threading.Tasks.Task.CompletedTask;
    };
});

// Antiforgery — validates X-XSRF-TOKEN header on state-changing requests
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});

// CORS for Vite dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("ViteDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReleaseRepository, ReleaseRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IJiraSettingsRepository, JiraSettingsRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReleaseService, ReleaseService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJiraSettingsService, JiraSettingsService>();
builder.Services.AddScoped<IJiraService, JiraService>();

// Named HttpClient for Jira API
builder.Services.AddHttpClient("Jira");

var app = builder.Build();

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("ViteDev");
app.UseAuthentication();
app.UseAuthorization();

// Antiforgery validation for all state-changing requests
app.Use(async (context, next) =>
{
    if (!HttpMethods.IsGet(context.Request.Method) &&
        !HttpMethods.IsHead(context.Request.Method) &&
        !HttpMethods.IsOptions(context.Request.Method))
    {
        // Skip antiforgery on the login endpoint (user is not yet authenticated)
        var path = context.Request.Path.Value ?? string.Empty;
        if (!path.EndsWith("/auth/login", StringComparison.OrdinalIgnoreCase))
        {
            var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
            await antiforgery.ValidateRequestAsync(context);
        }
    }
    await next();
});

app.MapControllers();

// Apply migrations and seed data on startup
using var scope = app.Services.CreateScope();

var db          = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
db.Database.Migrate();
await DbSeeder.SeedAsync(userManager, roleManager, db);

await app.RunAsync();
