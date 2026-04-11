using Microsoft.EntityFrameworkCore;
using Modules.Quiz.Application.Common.Extensions;
using Modules.Quiz.Application.Question.Question.Dtos;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Core;

namespace Modules.Quiz.Application.Question.Question.Query;
internal class GetQuestionByIdQueryHandler(IQuestionRepository repository) : IQueryHandler<GetQuestionByIdQuery, Result<QuestionResponse>>
{
    public async Task<Result<QuestionResponse>> Handle(GetQuestionByIdQuery request, CancellationToken cancellationToken)
    {
        var questionDetails = await repository.FirstOrDefaultAsync(
            x => x.QuestionId == request.QuestionId,
            include: q => q.Include(question => question.Options));

        if (questionDetails == null)
        {
            return QuestionErrors.QuestionNotFound;
        }

        return questionDetails.ToResponse();
    }
}
