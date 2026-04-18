using FluentValidation;

namespace Modules.AI.Application.Generation.Commands.GenerateFromJobDescription;

public sealed class GenerateFromJobDescriptionCommandValidator
    : AbstractValidator<GenerateFromJobDescriptionCommand>
{
    private static readonly string[] ValidOutputTypes = ["question_set", "interview_prep"];

    public GenerateFromJobDescriptionCommandValidator()
    {
        RuleFor(x => x.JobTitle)
            .NotEmpty().WithMessage("Job title is required.")
            .MaximumLength(200).WithMessage("Job title must not exceed 200 characters.");

        RuleFor(x => x.JobDescription)
            .NotEmpty().WithMessage("Job description is required.")
            .MinimumLength(50).WithMessage("Job description must be at least 50 characters.");

        RuleFor(x => x.OutputType)
            .NotEmpty().WithMessage("Output type is required.")
            .Must(t => ValidOutputTypes.Contains(t))
            .WithMessage("Output type must be one of: question_set, interview_prep.");

        RuleFor(x => x.QuestionCount)
            .InclusiveBetween(10, 50)
            .WithMessage("Question count must be between 10 and 50.")
            .When(x => x.OutputType == "question_set");
    }
}
