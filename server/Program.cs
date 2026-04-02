using System.Security.Claims;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectManagement.Auth;
using ProjectManagement.Data;
using ProjectManagement.Middleware;
using ProjectManagement.Repositories;
using ProjectManagement.Repositories.Interfaces;
using ProjectManagement.Services;
using ProjectManagement.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Controllers + JSON enum string support
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

// Swagger with Bearer auth support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: eyJhbGci..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// EF Core – SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Prevent standard JWT short claim names (sub, name, role) from being
        // remapped to long Microsoft URI-based ClaimTypes names
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireExpirationTime = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            // Keep role and name resolution working with our ClaimTypes-based tokens
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirst(
                    System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;

                if (string.IsNullOrEmpty(jti))
                {
                    context.Fail("Token is missing jti claim.");
                    return;
                }

                var revokedTokenRepo = context.HttpContext.RequestServices
                    .GetRequiredService<IRevokedTokenRepository>();

                if (await revokedTokenRepo.IsRevokedAsync(jti))
                    context.Fail("Token has been revoked.");
            }
        };
    });

builder.Services.AddAuthorization();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReleaseRepository, ReleaseRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IRevokedTokenRepository, RevokedTokenRepository>();

// Services
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReleaseService, ReleaseService>();
builder.Services.AddScoped<ITaskService, TaskService>();

var app = builder.Build();

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Apply migrations and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

app.Run();
