using Microsoft.EntityFrameworkCore;
using Modules.Quiz.Core.QuestionAggregate;
using Modules.Quiz.Infrastructure.Data;
using QuestionEntity = Modules.Quiz.Core.QuestionAggregate.Question;

namespace Modules.Quiz.Infrastructure.Persistence;

internal sealed class QuestionRepository(QuestionModuleDbContext context) : BaseRepository<QuestionEntity>(context), IQuestionRepository
{
    public async Task<QuestionEntity?> GetByIdWithOptionsAsync(long questionId)
    {
        return await context.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.QuestionId == questionId);
    }
}
