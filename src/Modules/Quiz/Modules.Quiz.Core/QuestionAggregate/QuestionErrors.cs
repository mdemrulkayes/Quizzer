using Shared.Core;

namespace Modules.Quiz.Core.QuestionAggregate;
public struct QuestionErrors
{
    public static Error QuestionSetNotFound => Error.NotFound("QuestionSet.QuestionSetNotFound", "Question Set not found.");
    public static Error QuestionNotFound => Error.NotFound("Question.QuestionNotFound", "Question not found.");
    public static Error TagAlreadyAssigned => Error.Conflict("QuestionSet.TagAlreadyAssigned", "Tag is already assigned to this question set.");
    public static Error TagNotAssigned => Error.NotFound("QuestionSet.TagNotAssigned", "Tag is not assigned to this question set.");
    public static Error QuestionOptionNotFound => Error.NotFound("QuestionOption.NotFound", "Question option not found.");
    public static Error MustHaveAtLeastOneOption => Error.Validation("Question.MustHaveAtLeastOneOption", "Question must have at least one option.");
    public static Error MustHaveExactlyOneCorrectAnswer => Error.Validation("Question.MustHaveExactlyOneCorrectAnswer", "Question must have exactly one correct answer.");
}
