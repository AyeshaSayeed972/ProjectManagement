using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Entities;

namespace ProjectManagement.Configurations;

public class RevokedTokenConfiguration : IEntityTypeConfiguration<RevokedToken>
{
    public void Configure(EntityTypeBuilder<RevokedToken> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Jti)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(r => r.ExpiresAt)
               .IsRequired();
    }
}
