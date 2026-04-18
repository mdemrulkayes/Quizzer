using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Quiz.Core.QuestionAggregate;

namespace Modules.Quiz.Infrastructure.Data.Configuration;
internal sealed class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.Property(x => x.QuestionId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.OptionText)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.OptionIdentifier)
            .HasMaxLength(5);

        builder.HasQueryFilter(x => x.DeletedDate == null);
    }
}
