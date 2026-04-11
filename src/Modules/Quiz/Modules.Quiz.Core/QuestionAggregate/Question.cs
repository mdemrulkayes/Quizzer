using System.Collections.ObjectModel;
using Shared.Core;

namespace Modules.Quiz.Core.QuestionAggregate;
public sealed class Question : BaseAuditableEntity
{
    public long QuestionId { get; private set; }
    public string AskedQuestion { get; private set; }
    public string Discussion { get; private set; }

    public int? QuestionMark { get; private set; }

    public long QuestionSetId { get; private set; }
    public QuestionSet? QuestionSet { get; private set; }

    public IReadOnlyCollection<QuestionOption> Options => new ReadOnlyCollection<QuestionOption>(_questionOptions);

    internal List<QuestionOption> _questionOptions = [];

    private Question(string askedQuestion, string discussion = "", int? questionMark = null)
    {
        AskedQuestion = askedQuestion;
        Discussion = discussion;
        QuestionMark = questionMark;
    }

    public static Result<Question> Create(string askedQuestion, string discussion = "", int? mark = null)
    {
        return new Question(askedQuestion, discussion, mark);
    }

    public Result<Question> Update(string askedQuestion, string discussion = "", int? mark = null)
    {
        AskedQuestion = askedQuestion;
        Discussion = discussion;
        QuestionMark = mark;

        return this;
    }

    public void AddQuestionOptions(string optionText, bool isAnswer = false)
    {
        var questionOption = QuestionOption.AddQuestionOption(optionText, isAnswer).Value;
        if (questionOption != null)
            _questionOptions.Add(questionOption);
    }

    public bool RemoveOption(QuestionOption option)
    {
        return _questionOptions.Remove(option);
    }

    public void Delete()
    {
        IsDeleted = true;
    }
}
