using AutoMapper;
using Modules.Quiz.Application.Tag.Dtos;
using Modules.Quiz.Core.QuestionAggregate;
using Modules.Quiz.Core.Tag;
using Shared.Core;

namespace Modules.Quiz.Application.Question.QuestionSetTag;

public sealed record AssignTagToQuestionSetCommand(long QuestionSetId, long TagId) : ICommand<Result<TagResponse>>;

internal sealed class AssignTagToQuestionSetCommandHandler(
    IQuestionSetRepository questionSetRepository,
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : ICommandHandler<AssignTagToQuestionSetCommand, Result<TagResponse>>
{
    public async Task<Result<TagResponse>> Handle(AssignTagToQuestionSetCommand request, CancellationToken cancellationToken)
    {
        var questionSet = await questionSetRepository.GetByIdWithTagsAsync(request.QuestionSetId);
        if (questionSet is null)
            return QuestionErrors.QuestionSetNotFound;

        var tag = await tagRepository.FirstOrDefaultAsync(t => t.TagId == request.TagId);
        if (tag is null)
            return TagErrors.TagNotFound;

        var result = questionSet.AddTag(tag);
        if (!result.IsSuccess)
            return result.Error;

        questionSetRepository.Update(questionSet);
        await unitOfWork.CommitAsync(cancellationToken);

        return mapper.Map<TagResponse>(tag);
    }
}
