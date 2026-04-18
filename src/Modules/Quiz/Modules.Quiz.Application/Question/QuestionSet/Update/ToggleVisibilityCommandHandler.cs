using Microsoft.Extensions.DependencyInjection;
using Modules.Quiz.Application.Common.Extensions;
using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Core;

namespace Modules.Quiz.Application.Question.QuestionSet.Update;

internal sealed class ToggleVisibilityCommandHandler(
    IQuestionSetRepository repository,
    [FromKeyedServices(ModuleKeys.Quiz)] IUnitOfWork unitOfWork)
    : ICommandHandler<ToggleVisibilityCommand, Result<QuestionSetResponse>>
{
    public async Task<Result<QuestionSetResponse>> Handle(
        ToggleVisibilityCommand request, CancellationToken cancellationToken)
    {
        var set = await repository.FirstOrDefaultAsync(x => x.QuestionSetId == request.QuestionSetId);
        if (set is null)
            return Error.NotFound("QuestionSet.NotFound", $"Question set {request.QuestionSetId} not found.");

        var result = set.SetVisibility(request.IsPublic);
        if (!result.IsSuccess)
            return result.Error;

        repository.Update(set);
        await unitOfWork.CommitAsync(cancellationToken);

        return set.ToResponse();
    }
}
