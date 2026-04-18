using FluentValidation;

namespace Modules.AI.Application.Generation.Commands.GenerateQuestionSet;

public sealed class GenerateQuestionSetCommandValidator : AbstractValidator<GenerateQuestionSetCommand>
{
    private static readonly string[] ValidComplexities =
        ["beginner", "intermediate", "professional", "expert"];

    public GenerateQuestionSetCommandValidator()
    {
        RuleFor(x => x.Topics)
            .NotEmpty().WithMessage("At least one topic must be provided.");

        RuleForEach(x => x.Topics)
            .NotEmpty().WithMessage("Each topic must not be empty.");

        RuleFor(x => x.Complexity)
            .NotEmpty().WithMessage("Complexity is required.")
            .Must(c => ValidComplexities.Contains(c))
            .WithMessage("Complexity must be one of: beginner, intermediate, professional, expert.");

        RuleFor(x => x.QuestionCount)
            .InclusiveBetween(10, 50)
            .WithMessage("Question count must be between 10 and 50.");

        RuleFor(x => x.ExperienceYears)
            .NotNull()
            .WithMessage("Experience years is required for professional or expert complexity.")
            .GreaterThan(0)
            .WithMessage("Experience years must be greater than 0 for professional or expert complexity.")
            .When(x => x.Complexity is "professional" or "expert");
    }
}
