using AutoMapper;
using Modules.Quiz.Application.Tag.Dtos;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Core;

namespace Modules.Quiz.Application.Question.QuestionSetTag;

public sealed record GetQuestionSetTagsQuery(long QuestionSetId) : IQuery<Result<List<TagResponse>>>;

internal sealed class GetQuestionSetTagsQueryHandler(
    IQuestionSetRepository questionSetRepository,
    IMapper mapper)
    : IQueryHandler<GetQuestionSetTagsQuery, Result<List<TagResponse>>>
{
    public async Task<Result<List<TagResponse>>> Handle(GetQuestionSetTagsQuery request, CancellationToken cancellationToken)
    {
        var questionSet = await questionSetRepository.GetByIdWithTagsAsync(request.QuestionSetId);
        if (questionSet is null)
            return QuestionErrors.QuestionSetNotFound;

        var tags = questionSet.QuestionSetTags.Select(qst => mapper.Map<TagResponse>(qst.Tag)).ToList();

        return tags;
    }
}
