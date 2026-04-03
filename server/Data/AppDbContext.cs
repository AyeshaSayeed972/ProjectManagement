using Microsoft.EntityFrameworkCore;
using ProjectManagement.Configurations;
using ProjectManagement.Entities;

namespace ProjectManagement.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Release> Releases => Set<Release>();
    public DbSet<Entities.Task> Tasks => Set<Entities.Task>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ReleaseConfiguration());
        modelBuilder.ApplyConfiguration(new TaskConfiguration());
        modelBuilder.ApplyConfiguration(new RevokedTokenConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
    }
}
