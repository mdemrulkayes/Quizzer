using Modules.Quiz.Application.Tag.Dtos;
using Shared.Application;
using Shared.Core;

namespace Modules.Quiz.Application.Tag.Query;

public sealed record GetAllTagQuery(
    string? SearchName = null,
    int PageNumber = 1,
    int PageSize = 10)
    : QueryStringParameter(PageNumber, PageSize), IQuery<Result<PagedListDto<TagResponse>>>;
