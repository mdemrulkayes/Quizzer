using Modules.Quiz.Core.Enums;
using Shared.Core;
using TagCore = Modules.Quiz.Core.Tag.Tag;

namespace Modules.Quiz.Core.QuestionAggregate;

public sealed class QuestionSet : BaseAuditableEntity, IAggregateRoot
{
    public long QuestionSetId { get; private set; }
    public string Name { get; private set; }
    public string? SetCode { get; private set; }
    public string? Details { get; private set; }
    public QuestionSetSource Source { get; private set; } = QuestionSetSource.Manual;
    public bool IsPublic { get; private set; } = false;
    public Complexity? Complexity { get; private set; }
    public int? ExperienceYears { get; private set; }
    public string? ExpertiseFields { get; private set; }

    public IReadOnlyCollection<QuestionSetTag> QuestionSetTags => _questionSetTags;

    internal List<QuestionSetTag> _questionSetTags = [];

    public IReadOnlyCollection<Question> Questions => _questions;

    internal List<Question> _questions = [];

    private QuestionSet(string name, string? setCode = "", string? details = "",
        QuestionSetSource source = QuestionSetSource.Manual, bool isPublic = false,
        Complexity? complexity = null, int? experienceYears = null, string? expertiseFields = null)
    {
        Name = name;
        SetCode = setCode;
        Details = details;
        Source = source;
        IsPublic = isPublic;
        Complexity = complexity;
        ExperienceYears = experienceYears;
        ExpertiseFields = expertiseFields;
    }

    public static Result<QuestionSet> Create(string name, string? setCode, string? details,
        QuestionSetSource source = QuestionSetSource.Manual, bool isPublic = false,
        Complexity? complexity = null, int? experienceYears = null, string? expertiseFields = null)
    {
        return new QuestionSet(name, setCode, details, source, isPublic, complexity, experienceYears, expertiseFields);
    }

    public Result<QuestionSet> Update(string name, string? setCode, string? details)
    {
        Name = name;
        SetCode = setCode;
        Details = details;

        return this;
    }

    public Result<QuestionSet> SetVisibility(bool isPublic)
    {
        IsPublic = isPublic;
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
            AddQuestion(question.AskedQuestion, [.. question.Options], question.Discussion, question.QuestionMark,
                question.QuestionType, question.Explanation, question.DifficultyScore, question.Sequence);
        }
    }

    private void AddQuestion(string askedQuestion, List<QuestionOption> questionOptions, string discussion = "", int? mark = null,
        QuestionType questionType = QuestionType.MultipleChoice, string? explanation = null,
        int? difficultyScore = null, int? sequence = null)
    {
        var addedQuestion = Question.Create(askedQuestion, discussion, mark, questionType, explanation, difficultyScore, sequence);

        if (addedQuestion.Value == null) return;

        var question = addedQuestion.Value;
        foreach (var option in questionOptions)
        {
            question.AddQuestionOptions(option.OptionText, option.IsAnswer, option.OptionIdentifier);
        }
        _questions.Add(question);
    }
}
