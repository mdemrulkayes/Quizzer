using Microsoft.EntityFrameworkCore;
using Modules.Quiz.Application.Common.Extensions;
using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Core;

namespace Modules.Quiz.Application.Question.QuestionSet.Query;
internal class GetQuestionSetByIdQueryHandler(IQuestionSetRepository repository) : IQueryHandler<GetQuestionSetByIdQuery, Result<QuestionSetResponse>>
{
    public async Task<Result<QuestionSetResponse>> Handle(GetQuestionSetByIdQuery request, CancellationToken cancellationToken)
    {
        var setDetails = await repository.FirstOrDefaultAsync(
            x => x.QuestionSetId == request.QuestionSetId,
            include: q => q
                .Include(qs => qs.Questions)
                    .ThenInclude(question => question.Options)
                .Include(qs => qs.QuestionSetTags));

        if (setDetails == null)
        {
            return QuestionErrors.QuestionSetNotFound;
        }

        return setDetails.ToResponse();
    }
}
