using Modules.Quiz.Application.Question.Question.Dtos;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Core;

namespace Modules.Quiz.Application.Question.QuestionOption;

public sealed record UpdateOptionCommand(long QuestionId, long OptionId, string OptionText, bool IsAnswer) : ICommand<Result<QuestionOptionResponse>>;

internal sealed class UpdateOptionCommandHandler(
    IQuestionRepository questionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateOptionCommand, Result<QuestionOptionResponse>>
{
    public async Task<Result<QuestionOptionResponse>> Handle(UpdateOptionCommand request, CancellationToken cancellationToken)
    {
        var question = await questionRepository.GetByIdWithOptionsAsync(request.QuestionId);
        if (question is null)
            return QuestionErrors.QuestionNotFound;

        var option = question.Options.FirstOrDefault(o => o.QuestionOptionId == request.OptionId);
        if (option is null)
            return QuestionErrors.QuestionOptionNotFound;

        option.Update(request.OptionText, request.IsAnswer);
        questionRepository.Update(question);
        await unitOfWork.CommitAsync(cancellationToken);

        return new QuestionOptionResponse(option.QuestionOptionId, option.OptionText, option.IsAnswer);
    }
}
