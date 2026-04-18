using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Modules.Quiz.Application.Common.Extensions;
using Modules.Quiz.Application.Question.QuestionSet.Dtos;
using Modules.Quiz.Core.Enums;
using Modules.Quiz.Core.QuestionAggregate;
using Shared.Application;
using Shared.Core;

namespace Modules.Quiz.Application.Question.QuestionSet.Query;
internal sealed class GetAllQuestionSetQueryHandler(IQuestionSetRepository repository)
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

        if (request.Source.HasValue)
        {
            var source = request.Source.Value;
            Expression<Func<Core.QuestionAggregate.QuestionSet, bool>> sourceFilter =
                qs => qs.Source == source;
            filter = filter is null ? sourceFilter : CombineExpressions(filter, sourceFilter);
        }

        if (request.IsPublic.HasValue)
        {
            var isPublic = request.IsPublic.Value;
            Expression<Func<Core.QuestionAggregate.QuestionSet, bool>> visibilityFilter =
                qs => qs.IsPublic == isPublic;
            filter = filter is null ? visibilityFilter : CombineExpressions(filter, visibilityFilter);
        }

        if (request.ComplexityFilter.HasValue)
        {
            var complexity = request.ComplexityFilter.Value;
            Expression<Func<Core.QuestionAggregate.QuestionSet, bool>> complexityFilter =
                qs => qs.Complexity == complexity;
            filter = filter is null ? complexityFilter : CombineExpressions(filter, complexityFilter);
        }

        var sets = await repository.GetAllAsync(
            expression: filter,
            include: q => q
                .Include(qs => qs.Questions)
                .Include(qs => qs.QuestionSetTags),
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        return sets.ToPagedListDto(s => s.ToResponse());
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
