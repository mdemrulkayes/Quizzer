using Microsoft.Extensions.DependencyInjection;
using Modules.Quiz.Application.Question.Question.Dtos;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Question.QuestionOption;

public sealed record AddOptionCommand(long QuestionId, string OptionText, bool IsAnswer) : ICommand<Result<QuestionOptionResponse>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate =>
    [
        $"{CacheKeys.Questions}:all:",
        $"{CacheKeys.Questions}:id:{QuestionId}",
        $"{CacheKeys.QuestionSets}:all:",
    ];
}

internal sealed class AddOptionCommandHandler(
    IQuestionRepository questionRepository,
    [FromKeyedServices(ModuleKeys.Quiz)] IUnitOfWork unitOfWork)
    : ICommandHandler<AddOptionCommand, Result<QuestionOptionResponse>>
{
    public async Task<Result<QuestionOptionResponse>> Handle(AddOptionCommand request, CancellationToken cancellationToken)
    {
        var question = await questionRepository.GetByIdWithOptionsAsync(request.QuestionId);
        if (question is null)
            return QuestionErrors.QuestionNotFound;

        question.AddQuestionOptions(request.OptionText, request.IsAnswer);
        questionRepository.Update(question);
        await unitOfWork.CommitAsync(cancellationToken);

        var addedOption = question.Options.Last();
        return new QuestionOptionResponse(addedOption.QuestionOptionId, addedOption.OptionText, addedOption.IsAnswer);
    }
}
