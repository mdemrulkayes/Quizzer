using FluentValidation;

namespace Modules.Exam.Application.Features.ExamManagement.Create;

public sealed class CreateExamCommandValidator : AbstractValidator<CreateExamCommand>
{
    public CreateExamCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.QuestionSetId)
            .GreaterThan(0).WithMessage("Question set ID must be greater than 0");

        RuleFor(x => x.DurationInMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than 0 minutes");

        RuleFor(x => x.TotalMarks)
            .GreaterThan(0).WithMessage("Total marks must be greater than 0");

        RuleFor(x => x.PassingMarks)
            .GreaterThanOrEqualTo(0).WithMessage("Passing marks must be 0 or greater")
            .LessThanOrEqualTo(x => x.TotalMarks).WithMessage("Passing marks cannot exceed total marks");
    }
}
