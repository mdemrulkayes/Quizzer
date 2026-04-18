using Microsoft.Extensions.DependencyInjection;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Question.QuestionSetTag;

public sealed record RemoveTagFromQuestionSetCommand(long QuestionSetId, long TagId) : ICommand<Result<bool>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate =>
    [
        $"{CacheKeys.QuestionSets}:all:",
        $"{CacheKeys.QuestionSets}:id:{QuestionSetId}",
    ];
}

internal sealed class RemoveTagFromQuestionSetCommandHandler(
    IQuestionSetRepository questionSetRepository,
    [FromKeyedServices(ModuleKeys.Quiz)] IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveTagFromQuestionSetCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RemoveTagFromQuestionSetCommand request, CancellationToken cancellationToken)
    {
        var questionSet = await questionSetRepository.GetByIdWithTagsAsync(request.QuestionSetId);
        if (questionSet is null)
            return QuestionErrors.QuestionSetNotFound;

        var result = questionSet.RemoveTag(request.TagId);
        if (!result.IsSuccess)
            return result.Error;

        questionSetRepository.Update(questionSet);
        await unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
