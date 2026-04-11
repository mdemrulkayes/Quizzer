using Microsoft.Extensions.DependencyInjection;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Core;

namespace Modules.Quiz.Application.Question.Question.Delete;
internal sealed class DeleteQuestionCommandHandler(IQuestionRepository repository, [FromKeyedServices(ModuleKeys.Quiz)] IUnitOfWork unitOfWork) : ICommandHandler<DeleteQuestionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken = default)
    {
        var question = await repository.FirstOrDefaultAsync(x => x.QuestionId == request.QuestionId);

        if (question == null)
        {
            return QuestionErrors.QuestionNotFound;
        }

        question.Delete();

        repository.Update(question);
        await unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
