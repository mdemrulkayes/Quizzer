using FluentValidation;
using Modules.Quiz.Core.QuestionAggregate;

namespace Modules.Quiz.Application.Question.Question.Create;
public sealed class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
{
    public CreateQuestionCommandValidator(IQuestionSetRepository repository)
    {
        RuleFor(x => x.Question)
            .NotEmpty()
            .WithMessage("Question can not be empty")
            .Length(5, 200)
            .WithMessage("Question can not be less than 5 characters and can not be more than 200 characters");

        RuleFor(x => x.Details)
            .Length(10, 150)
            .When(x => !string.IsNullOrWhiteSpace(x.Details))
            .WithMessage("Description can not be less than 10 characters and can not be more than 150 characters");

    }
}
