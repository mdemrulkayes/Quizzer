using AutoMapper;
using Modules.Quiz.Application.Question.Question.Create;
using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Core;

namespace Modules.Quiz.Application.Question.QuestionSet.Create;
internal sealed class CreateQuestionSetCommandHandler(IQuestionSetRepository repository, IUnitOfWork unitOfWork, IMapper mapper) : ICommandHandler<CreateQuestionSetCommand, Result<QuestionSetResponse>>
{
    /// <summary>Handles a request</summary>
    /// <param name="command">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response from the request</returns>
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

        return mapper.Map<Core.QuestionAggregate.QuestionSet, QuestionSetResponse>(set);
    }

    /// <summary>
    /// Map Question command to questions
    /// </summary>
    /// <param name="commands"></param>
    /// <returns></returns>
    private List<Core.QuestionAggregate.Question> MapQuestionCommandsToQuestions(List<CreateQuestionCommand> commands)
    {
        return [.. commands.Select(cmd =>
        {
            return Core.QuestionAggregate.Question.Create(cmd.Question, cmd.Details, cmd.Mark).Value!;
        })];
    }
}
