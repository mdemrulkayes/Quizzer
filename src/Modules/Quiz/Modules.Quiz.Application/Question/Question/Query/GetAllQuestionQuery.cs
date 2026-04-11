using Modules.Quiz.Application.Question.Question.Dtos;
using Shared.Application;
using Shared.Core;

namespace Modules.Quiz.Application.Question.Question.Query;

public sealed record GetAllQuestionQuery(
    string? SearchText = null,
    long? QuestionSetId = null,
    int PageNumber = 1,
    int PageSize = 10)
    : QueryStringParameter(PageNumber, PageSize), IQuery<Result<PagedListDto<QuestionResponse>>>;
