using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Quiz.Core.Enums;

namespace Modules.Quiz.Infrastructure.Data.Configuration;
internal sealed class QuestionConfiguration : IEntityTypeConfiguration<Core.QuestionAggregate.Question>
{
    public void Configure(EntityTypeBuilder<Core.QuestionAggregate.Question> builder)
    {
        builder.Property(x => x.QuestionId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.AskedQuestion)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.Discussion)
            .HasMaxLength(600);

        builder.Property(x => x.QuestionType)
            .HasConversion<int>()
            .HasDefaultValue(QuestionType.MultipleChoice);

        builder.Property(x => x.Explanation)
            .HasMaxLength(1000);

        builder.Property(x => x.DifficultyScore);

        builder.Property(x => x.Sequence);
    }
}
