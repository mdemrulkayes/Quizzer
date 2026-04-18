using Microsoft.Extensions.DependencyInjection;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Question.QuestionOption;

public sealed record DeleteOptionCommand(long QuestionId, long OptionId) : ICommand<Result<bool>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate =>
    [
        $"{CacheKeys.Questions}:all:",
        $"{CacheKeys.Questions}:id:{QuestionId}",
        $"{CacheKeys.QuestionSets}:all:",
    ];
}

internal sealed class DeleteOptionCommandHandler(
    IQuestionRepository questionRepository,
    [FromKeyedServices(ModuleKeys.Quiz)] IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteOptionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteOptionCommand request, CancellationToken cancellationToken)
    {
        var question = await questionRepository.GetByIdWithOptionsAsync(request.QuestionId);
        if (question is null)
            return QuestionErrors.QuestionNotFound;

        var option = question.Options.FirstOrDefault(o => o.QuestionOptionId == request.OptionId);
        if (option is null)
            return QuestionErrors.QuestionOptionNotFound;

        if (question.Options.Count <= 1)
            return QuestionErrors.MustHaveAtLeastOneOption;

        question.RemoveOption(option);
        questionRepository.Update(question);
        await unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
