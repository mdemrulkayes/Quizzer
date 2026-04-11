using Microsoft.Extensions.DependencyInjection;
using Modules.Quiz.Application.Common.Extensions;
using Modules.Quiz.Application.Question.Question.Create;
using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Core;

namespace Modules.Quiz.Application.Question.QuestionSet.Create;
internal sealed class CreateQuestionSetCommandHandler(IQuestionSetRepository repository, [FromKeyedServices(ModuleKeys.Quiz)] IUnitOfWork unitOfWork) : ICommandHandler<CreateQuestionSetCommand, Result<QuestionSetResponse>>
{
    public async Task<Result<QuestionSetResponse>> Handle(CreateQuestionSetCommand command, CancellationToken cancellationToken)
    {
        var questionSet = Core.QuestionAggregate.QuestionSet.Create(command.Name, command.SetCode, command.Details);

        if (!questionSet.IsSuccess || questionSet.Value is null)
        {
            return questionSet.Error;
        }

        var set = questionSet.Value;
        set.AddQuestions(MapQuestionCommandsToQuestions(command.Questions));

        repository.Add(set);
        await unitOfWork.CommitAsync(cancellationToken);

        return set.ToResponse();
    }

    private List<Core.QuestionAggregate.Question> MapQuestionCommandsToQuestions(List<CreateQuestionCommand> commands)
    {
        return [.. commands.Select(cmd =>
        {
            var question = Core.QuestionAggregate.Question.Create(cmd.Question, cmd.Details, cmd.Mark).Value!;
            foreach (var option in cmd.QuestionOptions)
            {
                question.AddQuestionOptions(option.OptionText, option.IsAnswer);
            };
            return question;
        })];
    }
}
