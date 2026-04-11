using System.Linq.Expressions;
using AutoMapper;
using Modules.Quiz.Application.Question.Question.Dtos;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Application;
using Shared.Core;
using QuestionEntity = Modules.Quiz.Core.QuestionAggregate.Question;

namespace Modules.Quiz.Application.Question.Question.Query;
internal sealed class GetAllQuestionQueryHandler(IQuestionRepository repository, IMapper mapper)
    : IQueryHandler<GetAllQuestionQuery, Result<PagedListDto<QuestionResponse>>>
{
    public async Task<Result<PagedListDto<QuestionResponse>>> Handle(GetAllQuestionQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<QuestionEntity, bool>>? filter = null;

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchTerm = request.SearchText.ToLower();
            filter = q => q.AskedQuestion.ToLower().Contains(searchTerm);
        }

        if (request.QuestionSetId.HasValue)
        {
            var setId = request.QuestionSetId.Value;
            Expression<Func<QuestionEntity, bool>> setFilter = q => q.QuestionSetId == setId;

            if (filter is null)
                filter = setFilter;
            else
            {
                var parameter = Expression.Parameter(typeof(QuestionEntity));
                var combined = Expression.AndAlso(
                    Expression.Invoke(filter, parameter),
                    Expression.Invoke(setFilter, parameter));
                filter = Expression.Lambda<Func<QuestionEntity, bool>>(combined, parameter);
            }
        }

        var questions = await repository.GetAllAsync(
            expression: filter,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        return mapper.Map<PagedListDto<QuestionResponse>>(questions);
    }
}
