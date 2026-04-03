using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectManagement.Configurations;

public class TaskConfiguration : IEntityTypeConfiguration<Entities.Task>
{
    public void Configure(EntityTypeBuilder<Entities.Task> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(t => t.PRLink)
               .HasMaxLength(500);

        builder.Property(t => t.Remarks)
               .HasMaxLength(1000);

        builder.Property(t => t.Status)
               .IsRequired()
               .HasConversion<string>();

        builder.Property(t => t.CreatedAt)
               .IsRequired();

        builder.HasOne(t => t.AssignedToUser)
               .WithMany(u => u.AssignedTasks)
               .HasForeignKey(t => t.AssignedToUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedToQAUser)
               .WithMany(u => u.QAAssignedTasks)
               .HasForeignKey(t => t.AssignedToQAUserId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);
    }
}
