using Microsoft.Extensions.DependencyInjection;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Core;
using Shared.Core.IntegrationEvents;
using Shared.Core.IntegrationEvents.Events;

namespace Modules.Quiz.Application.Question.QuestionSet.Delete;
internal sealed class DeleteQuestionSetCommandHandler(
    IQuestionSetRepository repository,
    [FromKeyedServices(ModuleKeys.Quiz)] IUnitOfWork unitOfWork,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<DeleteQuestionSetCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteQuestionSetCommand request, CancellationToken cancellationToken = default)
    {
        var questionSet = await repository.FirstOrDefaultAsync(x => x.QuestionSetId == request.QuestionSetId);

        if (questionSet == null)
        {
            return QuestionErrors.QuestionSetNotFound;
        }

        questionSet.Delete();
        repository.Update(questionSet);
        await unitOfWork.CommitAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new QuestionSetDeletedEvent(request.QuestionSetId), cancellationToken);

        return true;
    }
}
