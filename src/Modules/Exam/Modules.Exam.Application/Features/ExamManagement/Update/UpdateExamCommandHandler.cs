using Modules.Exam.Application.Features.ExamManagement.Dtos;
using Modules.Exam.Core.ExamAggregate;
using Shared.Core;

namespace Modules.Exam.Application.Features.ExamManagement.Update;

internal sealed class UpdateExamCommandHandler(
    IExamRepository examRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateExamCommand, Result<ExamResponse>>
{
    public async Task<Result<ExamResponse>> Handle(UpdateExamCommand command, CancellationToken cancellationToken)
    {
        var exam = await examRepository.FirstOrDefaultAsync(e => e.ExamId == command.ExamId);
        if (exam is null)
            return ExamErrors.ExamNotFound;

        var updateResult = exam.Update(
            command.Title, command.Description, command.DurationInMinutes,
            command.TotalMarks, command.PassingMarks,
            command.ScheduledStartTime, command.ScheduledEndTime);

        if (!updateResult.IsSuccess)
            return updateResult.Error;

        examRepository.Update(exam);
        await unitOfWork.CommitAsync(cancellationToken);

        return new ExamResponse(
            exam.ExamId, exam.Title, exam.Description, exam.QuestionSetId,
            exam.DurationInMinutes, exam.TotalMarks, exam.PassingMarks,
            exam.IsPublished, exam.ScheduledStartTime, exam.ScheduledEndTime);
    }
}
