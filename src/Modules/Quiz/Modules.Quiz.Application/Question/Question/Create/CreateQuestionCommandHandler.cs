using Microsoft.Extensions.DependencyInjection;
using Modules.Quiz.Application.Common.Extensions;
using Modules.Quiz.Application.Question.Question.Dtos;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Core;

namespace Modules.Quiz.Application.Question.Question.Create;
internal sealed class CreateQuestionCommandHandler(IQuestionRepository repository, [FromKeyedServices(ModuleKeys.Quiz)] IUnitOfWork unitOfWork) : ICommandHandler<CreateQuestionCommand, Result<QuestionResponse>>
{
    public async Task<Result<QuestionResponse>> Handle(CreateQuestionCommand command, CancellationToken cancellationToken)
    {
        var questionResult = Core.QuestionAggregate.Question.Create(command.Question, command.Details, command.Mark);

        if (!questionResult.IsSuccess || questionResult.Value is null)
        {
            return questionResult.Error;
        }

        var question = questionResult.Value;

        repository.Add(question);
        await unitOfWork.CommitAsync(cancellationToken);

        return question.ToResponse();
    }
}
