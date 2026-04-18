using Modules.Quiz.Core.Enums;
using Shared.Core;

namespace Modules.Quiz.Core.QuestionAggregate;
public sealed class Question : BaseAuditableEntity
{
    public long QuestionId { get; private set; }
    public string AskedQuestion { get; private set; }
    public string Discussion { get; private set; }

    public int? QuestionMark { get; private set; }
    public QuestionType QuestionType { get; private set; } = QuestionType.MultipleChoice;
    public string? Explanation { get; private set; }
    public int? DifficultyScore { get; private set; }
    public int? Sequence { get; private set; }

    public long QuestionSetId { get; private set; }
    public QuestionSet? QuestionSet { get; private set; }

    public IReadOnlyCollection<QuestionOption> Options => _questionOptions;

    internal List<QuestionOption> _questionOptions = [];

    private Question(string askedQuestion, string discussion = "", int? questionMark = null,
        QuestionType questionType = QuestionType.MultipleChoice, string? explanation = null,
        int? difficultyScore = null, int? sequence = null)
    {
        AskedQuestion = askedQuestion;
        Discussion = discussion;
        QuestionMark = questionMark;
        QuestionType = questionType;
        Explanation = explanation;
        DifficultyScore = difficultyScore;
        Sequence = sequence;
    }

    public static Result<Question> Create(string askedQuestion, string discussion = "", int? mark = null,
        QuestionType questionType = QuestionType.MultipleChoice, string? explanation = null,
        int? difficultyScore = null, int? sequence = null)
    {
        return new Question(askedQuestion, discussion, mark, questionType, explanation, difficultyScore, sequence);
    }

    public Result<Question> Update(string askedQuestion, string discussion = "", int? mark = null,
        QuestionType questionType = QuestionType.MultipleChoice, string? explanation = null,
        int? difficultyScore = null, int? sequence = null)
    {
        AskedQuestion = askedQuestion;
        Discussion = discussion;
        QuestionMark = mark;
        QuestionType = questionType;
        Explanation = explanation;
        DifficultyScore = difficultyScore;
        Sequence = sequence;

        return this;
    }

    public void AddQuestionOptions(string optionText, bool isAnswer = false, string? optionIdentifier = null)
    {
        var questionOption = QuestionOption.AddQuestionOption(optionText, isAnswer, optionIdentifier).Value;
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
