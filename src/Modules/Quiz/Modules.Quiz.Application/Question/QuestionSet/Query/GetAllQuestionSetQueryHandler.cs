using System.Linq.Expressions;
using AutoMapper;
using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Application;
using Shared.Core;

namespace Modules.Quiz.Application.Question.QuestionSet.Query;
internal sealed class GetAllQuestionSetQueryHandler(IQuestionSetRepository repository, IMapper mapper)
    : IQueryHandler<GetAllQuestionSetQuery, Result<PagedListDto<QuestionSetResponse>>>
{
    public async Task<Result<PagedListDto<QuestionSetResponse>>> Handle(GetAllQuestionSetQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Core.QuestionAggregate.QuestionSet, bool>>? filter = null;

        if (!string.IsNullOrWhiteSpace(request.SearchName))
        {
            var searchTerm = request.SearchName.ToLower();
            filter = qs => qs.Name.ToLower().Contains(searchTerm);
        }

        if (request.TagId.HasValue)
        {
            var tagId = request.TagId.Value;
            Expression<Func<Core.QuestionAggregate.QuestionSet, bool>> tagFilter =
                qs => qs.QuestionSetTags.Any(qst => qst.TagId == tagId);

            filter = filter is null ? tagFilter : CombineExpressions(filter, tagFilter);
        }

        var sets = await repository.GetAllAsync(
            expression: filter,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        return mapper.Map<PagedListDto<QuestionSetResponse>>(sets);
    }

    private static Expression<Func<T, bool>> CombineExpressions<T>(
        Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        var parameter = Expression.Parameter(typeof(T));
        var combined = Expression.AndAlso(
            Expression.Invoke(first, parameter),
            Expression.Invoke(second, parameter));
        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }
}
