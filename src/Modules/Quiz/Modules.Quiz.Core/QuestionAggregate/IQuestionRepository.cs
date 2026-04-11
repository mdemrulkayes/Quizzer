using Shared.Core;

namespace Modules.Quiz.Core.QuestionAggregate;
public interface IQuestionRepository : IRepository<Question>
{
    Task<Question?> GetByIdWithOptionsAsync(long questionId);
}