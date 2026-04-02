using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Entities;

namespace ProjectManagement.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.TokenHash)
               .IsRequired()
               .HasMaxLength(64);

        builder.Property(r => r.ExpiresAt)
               .IsRequired();

        builder.Property(r => r.CreatedAt)
               .IsRequired();

        builder.Property(r => r.RevokedAt)
               .IsRequired(false);

        builder.Ignore(r => r.IsRevoked);
        builder.Ignore(r => r.IsExpired);
        builder.Ignore(r => r.IsActive);

        builder.HasOne(r => r.User)
               .WithMany()
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
