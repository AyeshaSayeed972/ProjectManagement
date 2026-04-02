using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Entities;

namespace ProjectManagement.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
               .IsRequired();

        builder.Property(u => u.Role)
               .IsRequired()
               .HasConversion<string>();
    }
}
