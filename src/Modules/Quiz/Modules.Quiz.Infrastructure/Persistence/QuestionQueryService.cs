using Microsoft.EntityFrameworkCore;
using Modules.Quiz.Infrastructure.Data;
using Shared.Core.ModuleServices;

namespace Modules.Quiz.Infrastructure.Persistence;

internal sealed class QuestionQueryService(QuestionModuleDbContext dbContext) : IQuestionQueryService
{
    public async Task<bool> QuestionSetExistsAsync(long questionSetId, CancellationToken cancellationToken = default)
    {
        return await dbContext.QuestionSets
            .AnyAsync(qs => qs.QuestionSetId == questionSetId, cancellationToken);
    }

    public async Task<IReadOnlyList<QuestionDto>> GetQuestionsBySetIdAsync(long questionSetId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Questions
            .Where(q => q.QuestionSetId == questionSetId)
            .Select(q => new QuestionDto(q.QuestionId, q.AskedQuestion, q.QuestionMark))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<QuestionOptionDto>> GetOptionsByQuestionIdsAsync(IEnumerable<long> questionIds, CancellationToken cancellationToken = default)
    {
        var ids = questionIds.ToList();
        return await dbContext.QuestionOptions
            .Where(qo => ids.Contains(qo.QuestionId))
            .Select(qo => new QuestionOptionDto(qo.QuestionOptionId, qo.OptionText, qo.QuestionId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CorrectAnswerDto>> GetCorrectAnswersBySetIdAsync(long questionSetId, CancellationToken cancellationToken = default)
    {
        return await dbContext.QuestionOptions
            .Where(qo => qo.Question.QuestionSetId == questionSetId && qo.IsAnswer)
            .Select(qo => new CorrectAnswerDto(qo.QuestionOptionId, qo.QuestionId))
            .ToListAsync(cancellationToken);
    }
}
