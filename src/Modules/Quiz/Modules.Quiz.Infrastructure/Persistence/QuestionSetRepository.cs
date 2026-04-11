using Microsoft.EntityFrameworkCore;
using Modules.Quiz.Core.QuestionAggregate;
using Modules.Quiz.Infrastructure.Data;

namespace Modules.Quiz.Infrastructure.Persistence;

internal sealed class QuestionSetRepository(QuestionModuleDbContext context) : BaseRepository<QuestionSet>(context), IQuestionSetRepository
{
    public async Task<QuestionSet?> GetByIdWithTagsAsync(long questionSetId)
    {
        return await context.QuestionSets
            .Include(qs => qs.QuestionSetTags)
                .ThenInclude(qst => qst.Tag)
            .FirstOrDefaultAsync(qs => qs.QuestionSetId == questionSetId);
    }
}
