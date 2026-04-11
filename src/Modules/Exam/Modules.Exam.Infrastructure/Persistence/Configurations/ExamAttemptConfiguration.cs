using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Exam.Core.ExamAggregate;

namespace Modules.Exam.Infrastructure.Persistence.Configurations;

internal sealed class ExamAttemptConfiguration : IEntityTypeConfiguration<ExamAttempt>
{
    public void Configure(EntityTypeBuilder<ExamAttempt> builder)
    {
        builder.ToTable("ExamAttempts");

        builder.HasKey(a => a.ExamAttemptId);
        builder.Property(a => a.ExamAttemptId).UseIdentityColumn();

        builder.Property(a => a.ExamId).IsRequired();
        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.StartedAt).IsRequired();
        builder.Property(a => a.Status).IsRequired();

        builder.HasMany(a => a.Answers)
            .WithOne(ans => ans.ExamAttempt)
            .HasForeignKey(ans => ans.ExamAttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(a => a.DeletedDate == null);
    }
}
