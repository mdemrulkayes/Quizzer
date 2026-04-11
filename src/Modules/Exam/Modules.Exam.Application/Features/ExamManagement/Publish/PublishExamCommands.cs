using Microsoft.Extensions.DependencyInjection;
using Modules.Exam.Core.ExamAggregate;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Exam.Application.Features.ExamManagement.Publish;

public sealed record PublishExamCommand(long ExamId) : ICommand<Result<bool>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate => [$"{CacheKeys.Exams}:id:{ExamId}"];
}

public sealed record UnpublishExamCommand(long ExamId) : ICommand<Result<bool>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate => [$"{CacheKeys.Exams}:id:{ExamId}"];
}

internal sealed class PublishExamCommandHandler(
    IExamRepository examRepository,
    [FromKeyedServices(ModuleKeys.Exam)] IUnitOfWork unitOfWork)
    : ICommandHandler<PublishExamCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(PublishExamCommand command, CancellationToken cancellationToken)
    {
        var exam = await examRepository.FirstOrDefaultAsync(e => e.ExamId == command.ExamId);
        if (exam is null)
            return ExamErrors.ExamNotFound;

        var result = exam.Publish();
        if (!result.IsSuccess)
            return result.Error;

        examRepository.Update(exam);
        await unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}

internal sealed class UnpublishExamCommandHandler(
    IExamRepository examRepository,
    [FromKeyedServices(ModuleKeys.Exam)] IUnitOfWork unitOfWork)
    : ICommandHandler<UnpublishExamCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UnpublishExamCommand command, CancellationToken cancellationToken)
    {
        var exam = await examRepository.FirstOrDefaultAsync(e => e.ExamId == command.ExamId);
        if (exam is null)
            return ExamErrors.ExamNotFound;

        var result = exam.Unpublish();
        if (!result.IsSuccess)
            return result.Error;

        examRepository.Update(exam);
        await unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
