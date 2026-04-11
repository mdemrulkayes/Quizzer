using Modules.Exam.Core.ExamAggregate;
using Shared.Core;

namespace Modules.Exam.Application.Features.ExamManagement.Delete;

public sealed record DeleteExamCommand(long ExamId) : ICommand<Result<bool>>;

internal sealed class DeleteExamCommandHandler(
    IExamRepository examRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteExamCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteExamCommand command, CancellationToken cancellationToken)
    {
        var exam = await examRepository.FirstOrDefaultAsync(e => e.ExamId == command.ExamId);
        if (exam is null)
            return ExamErrors.ExamNotFound;

        examRepository.Delete(exam);
        await unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
