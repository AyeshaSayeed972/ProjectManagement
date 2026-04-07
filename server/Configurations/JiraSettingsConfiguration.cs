using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Entities;

namespace ProjectManagement.Configurations;

public class JiraSettingsConfiguration : IEntityTypeConfiguration<JiraSettings>
{
    public void Configure(EntityTypeBuilder<JiraSettings> builder)
    {
        builder.HasKey(j => j.Id);

        builder.Property(j => j.BaseUrl)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(j => j.Email)
               .IsRequired()
               .HasMaxLength(254);

        builder.Property(j => j.ApiToken)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(j => j.UpdatedAt)
               .IsRequired();
    }
}
