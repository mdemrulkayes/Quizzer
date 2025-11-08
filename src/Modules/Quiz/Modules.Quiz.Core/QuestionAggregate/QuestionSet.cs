using System.Collections.ObjectModel;
using Shared.Core;

namespace Modules.Quiz.Core.QuestionAggregate;

public sealed class QuestionSet : BaseAuditableEntity, IAggregateRoot
{
    public long QuestionSetId { get; private set; }
    public string Name { get; private set; }
    public string? SetCode { get; private set; }
    public string? Details { get; private set; }

    public IReadOnlyCollection<QuestionSetTag> QuestionSetTags => new ReadOnlyCollection<QuestionSetTag>(_questionSetTags);

    internal List<QuestionSetTag> _questionSetTags = [];

    public IReadOnlyCollection<Question> Questions => new ReadOnlyCollection<Question>(_questions);

    internal List<Question> _questions = [];

    private QuestionSet(string name, string? setCode = "", string? details = "")
    {
        Name = name;
        SetCode = setCode;
        Details = details;
    }

    public static Result<QuestionSet> Create(string name, string? setCode, string? details)
    {
        return new QuestionSet(name, setCode, details);
    }

    public Result<QuestionSet> Update(string name, string? setCode, string? details)
    {
        Name = name;
        SetCode = setCode;
        Details = details;

        return this;
    }

    public void AddQuestions(List<Question> questions)
    {
        foreach (var question in questions)
        {
            AddQuestion(question.AskedQuestion, [.. question.Options], question.Discussion, question.QuestionMark);
        }
    }

    private void AddQuestion(string askedQuestion, List<QuestionOption> questionOptions, string discussion = "", int? mark = null)
    {
        var addedQuestion = Question.Create(askedQuestion, discussion, mark);

        if (addedQuestion.Value == null) return;

        var question = addedQuestion.Value;
        foreach (var option in questionOptions)
        {
            question.AddQuestionOptions(option.OptionText, option.IsAnswer);
        }
        _questions.Add(question);
    }
}
