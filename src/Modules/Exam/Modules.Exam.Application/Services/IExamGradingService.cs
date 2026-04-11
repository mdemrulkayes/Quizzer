using Modules.Exam.Core.ExamAggregate;

namespace Modules.Exam.Application.Services;

public interface IExamGradingService
{
    Task GradeAttemptAsync(ExamAttempt attempt, Core.ExamAggregate.Exam exam, CancellationToken cancellationToken = default);
}
