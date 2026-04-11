using Shared.Core;

namespace Modules.Quiz.Core.QuestionAggregate;
public sealed class QuestionOption : BaseAuditableEntity
{
    public long QuestionOptionId { get; private set; }
    public string OptionText { get; private set; }
    public long QuestionId { get; private set; }
    public bool IsAnswer { get; private set; }

    public Question Question { get; private set; }

    private QuestionOption(string optionText, bool isAnswer = false)
    {
        OptionText = optionText;
        IsAnswer = isAnswer;
    }

    public static Result<QuestionOption> AddQuestionOption(string text, bool isAnswer)
    {
        return new QuestionOption(text, isAnswer);
    }

    public void Update(string optionText, bool isAnswer)
    {
        OptionText = optionText;
        IsAnswer = isAnswer;
    }
}
