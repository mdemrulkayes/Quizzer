using Microsoft.Extensions.DependencyInjection;
using Modules.Quiz.Application.Common.Extensions;
using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Core;

namespace Modules.Quiz.Application.Question.QuestionSet.Update;
internal sealed class UpdateQuestionSetCommandHandler(IQuestionSetRepository repository,
    [FromKeyedServices(ModuleKeys.Quiz)] IUnitOfWork unitOfWork) : ICommandHandler<UpdateQuestionSetCommand, Result<QuestionSetResponse>>
{
    public async Task<Result<QuestionSetResponse>> Handle(UpdateQuestionSetCommand request, CancellationToken cancellationToken = default)
    {
        var set = await repository.FirstOrDefaultAsync(x => x.QuestionSetId == request.QuestionSetId);
        if (set == null)
        {
            return QuestionErrors.QuestionSetNotFound;
        }

        var updatedSet = set.Update(request.Name, request.SetCode, request.Details);

        if (!updatedSet.IsSuccess || updatedSet.Value is null)
        {
            return updatedSet.Error;
        }

        repository.Update(updatedSet.Value);
        await unitOfWork.CommitAsync(cancellationToken);

        return updatedSet.Value.ToResponse();
    }
}
