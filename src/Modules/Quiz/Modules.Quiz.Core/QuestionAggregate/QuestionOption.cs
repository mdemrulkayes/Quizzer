using Shared.Core;

namespace Modules.Quiz.Core.QuestionAggregate;
public sealed class QuestionOption : BaseAuditableEntity
{
    public long QuestionOptionId { get; private set; }
    public string OptionText { get; private set; }
    public long QuestionId { get; private set; }
    public bool IsAnswer { get; private set; }
    public string? OptionIdentifier { get; private set; }

    public Question Question { get; private set; }

    private QuestionOption(string optionText, bool isAnswer = false, string? optionIdentifier = null)
    {
        OptionText = optionText;
        IsAnswer = isAnswer;
        OptionIdentifier = optionIdentifier;
    }

    public static Result<QuestionOption> AddQuestionOption(string text, bool isAnswer, string? optionIdentifier = null)
    {
        return new QuestionOption(text, isAnswer, optionIdentifier);
    }

    public void Update(string optionText, bool isAnswer, string? optionIdentifier = null)
    {
        OptionText = optionText;
        IsAnswer = isAnswer;
        OptionIdentifier = optionIdentifier;
    }
}
