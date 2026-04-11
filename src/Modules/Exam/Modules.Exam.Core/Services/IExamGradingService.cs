using Modules.Exam.Core.ExamAggregate;

namespace Modules.Exam.Core.Services;

public interface IExamGradingService
{
    Task GradeAttemptAsync(ExamAttempt attempt, ExamAggregate.Exam exam, CancellationToken cancellationToken = default);
}
