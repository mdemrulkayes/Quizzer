using Microsoft.Extensions.DependencyInjection;
using Modules.Exam.Core.ExamAggregate;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Exam.Application.Features.ExamManagement.Delete;

public sealed record DeleteExamCommand(long ExamId) : ICommand<Result<bool>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate => [$"{CacheKeys.Exams}:id:{ExamId}"];
}

internal sealed class DeleteExamCommandHandler(
    IExamRepository examRepository,
    [FromKeyedServices(ModuleKeys.Exam)] IUnitOfWork unitOfWork)
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
