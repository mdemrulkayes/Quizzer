using Shared.Core;

namespace Modules.Exam.Core.ExamAggregate;

public struct ExamErrors
{
    public static Error ExamNotFound => Error.NotFound("Exam.NotFound", "Exam not found");
    public static Error TitleRequired => Error.Validation("Exam.TitleRequired", "Exam title is required");
    public static Error InvalidDuration => Error.Validation("Exam.InvalidDuration", "Duration must be greater than 0");
    public static Error PassingMarksExceedTotal => Error.Validation("Exam.PassingMarksExceedTotal", "Passing marks cannot exceed total marks");
    public static Error CannotModifyPublishedExam => Error.Failure("Exam.CannotModifyPublished", "Cannot modify a published exam");
    public static Error AlreadyPublished => Error.Failure("Exam.AlreadyPublished", "Exam is already published");
    public static Error NotPublished => Error.Failure("Exam.NotPublished", "Exam is not published");
    public static Error ExamNotPublished => Error.Failure("Exam.NotAvailable", "Exam is not available for taking");
    public static Error ExamNotInSchedule => Error.Failure("Exam.NotInSchedule", "Exam is not within its scheduled time window");
    public static Error AttemptAlreadyInProgress => Error.Conflict("Exam.AttemptAlreadyInProgress", "You already have an in-progress attempt for this exam");
    public static Error AttemptNotFound => Error.NotFound("Exam.AttemptNotFound", "Exam attempt not found");
    public static Error AttemptNotInProgress => Error.Failure("Exam.AttemptNotInProgress", "Exam attempt is not in progress");
    public static Error AttemptExpired => Error.Failure("Exam.AttemptExpired", "Exam attempt has expired");
    public static Error QuestionNotInExam => Error.Validation("Exam.QuestionNotInExam", "Question does not belong to this exam's question set");
    public static Error QuestionSetNotFound => Error.NotFound("Exam.QuestionSetNotFound", "Question set not found for this exam");
}
