using Shared.Core;

namespace Modules.Quiz.Core.QuestionAggregate;
public interface IQuestionSetRepository : IRepository<QuestionSet>
{
    Task<QuestionSet?> GetByIdWithTagsAsync(long questionSetId);
}
