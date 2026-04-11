using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modules.Exam.Infrastructure.Persistence.Configurations;

internal sealed class ExamConfiguration : IEntityTypeConfiguration<Core.ExamAggregate.Exam>
{
    public void Configure(EntityTypeBuilder<Core.ExamAggregate.Exam> builder)
    {
        builder.ToTable("Exams");

        builder.HasKey(e => e.ExamId);
        builder.Property(e => e.ExamId).UseIdentityColumn();

        builder.Property(e => e.Title).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.QuestionSetId).IsRequired();
        builder.Property(e => e.DurationInMinutes).IsRequired();
        builder.Property(e => e.TotalMarks).IsRequired();
        builder.Property(e => e.PassingMarks).IsRequired();
        builder.Property(e => e.IsPublished).IsRequired().HasDefaultValue(false);

        builder.HasMany(e => e.Attempts)
            .WithOne(a => a.Exam)
            .HasForeignKey(a => a.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => e.DeletedDate == null);
    }
}
