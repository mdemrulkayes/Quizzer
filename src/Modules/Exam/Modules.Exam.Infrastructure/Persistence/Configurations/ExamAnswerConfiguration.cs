using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Exam.Core.ExamAggregate;

namespace Modules.Exam.Infrastructure.Persistence.Configurations;

internal sealed class ExamAnswerConfiguration : IEntityTypeConfiguration<ExamAnswer>
{
    public void Configure(EntityTypeBuilder<ExamAnswer> builder)
    {
        builder.ToTable("ExamAnswers");

        builder.HasKey(a => a.ExamAnswerId);
        builder.Property(a => a.ExamAnswerId).UseIdentityColumn();

        builder.Property(a => a.ExamAttemptId).IsRequired();
        builder.Property(a => a.QuestionId).IsRequired();

        builder.HasQueryFilter(a => a.DeletedDate == null);
    }
}
