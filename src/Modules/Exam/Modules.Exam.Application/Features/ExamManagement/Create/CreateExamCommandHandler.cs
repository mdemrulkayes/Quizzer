using Modules.Exam.Application.Features.ExamManagement.Dtos;
using Modules.Exam.Core.ExamAggregate;
using Shared.Core;

namespace Modules.Exam.Application.Features.ExamManagement.Create;

internal sealed class CreateExamCommandHandler(
    IExamRepository examRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateExamCommand, Result<ExamResponse>>
{
    public async Task<Result<ExamResponse>> Handle(CreateExamCommand command, CancellationToken cancellationToken)
    {
        var examResult = Core.ExamAggregate.Exam.Create(
            command.Title,
            command.Description,
            command.QuestionSetId,
            command.DurationInMinutes,
            command.TotalMarks,
            command.PassingMarks,
            command.ScheduledStartTime,
            command.ScheduledEndTime);

        if (!examResult.IsSuccess)
            return examResult.Error;

        var exam = examResult.Value!;
        examRepository.Add(exam);
        await unitOfWork.CommitAsync(cancellationToken);

        return new ExamResponse(
            exam.ExamId, exam.Title, exam.Description, exam.QuestionSetId,
            exam.DurationInMinutes, exam.TotalMarks, exam.PassingMarks,
            exam.IsPublished, exam.ScheduledStartTime, exam.ScheduledEndTime);
    }
}
