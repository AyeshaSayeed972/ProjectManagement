using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Entities;

namespace ProjectManagement.Configurations;

public class ReleaseConfiguration : IEntityTypeConfiguration<Release>
{
    public void Configure(EntityTypeBuilder<Release> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(r => r.Description)
               .HasMaxLength(1000);

        builder.Property(r => r.StartDate)
               .IsRequired();

        builder.Property(r => r.EndDate)
               .IsRequired();

        builder.Property(r => r.Status)
               .IsRequired()
               .HasConversion<string>();

        builder.Property(r => r.CreatedAt)
               .IsRequired();

        builder.HasMany(r => r.Tasks)
               .WithOne(t => t.Release)
               .HasForeignKey(t => t.ReleaseId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
