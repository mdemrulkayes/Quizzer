using System.Collections.ObjectModel;
using Shared.Core;
using TagCore = Modules.Quiz.Core.Tag.Tag;

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

    public void Delete()
    {
        IsDeleted = true;
    }

    public Result<QuestionSetTag> AddTag(TagCore tag)
    {
        if (_questionSetTags.Any(t => t.TagId == tag.TagId))
            return QuestionErrors.TagAlreadyAssigned;

        var questionSetTag = new QuestionSetTag(tag, this);
        _questionSetTags.Add(questionSetTag);
        return questionSetTag;
    }

    public Result<bool> RemoveTag(long tagId)
    {
        var questionSetTag = _questionSetTags.FirstOrDefault(t => t.TagId == tagId);
        if (questionSetTag is null)
            return QuestionErrors.TagNotAssigned;

        _questionSetTags.Remove(questionSetTag);
        return true;
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
