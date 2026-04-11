using System.Linq.Expressions;
using Modules.Quiz.Application.Common.Extensions;
using Modules.Quiz.Application.Tag.Dtos;
using Modules.Quiz.Core.Tag;
using Shared.Application;
using Shared.Core;

namespace Modules.Quiz.Application.Tag.Query;
internal sealed class GetAllTagQueryHandler(ITagRepository tagRepository)
    : IQueryHandler<GetAllTagQuery, Result<PagedListDto<TagResponse>>>
{
    public async Task<Result<PagedListDto<TagResponse>>> Handle(GetAllTagQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Core.Tag.Tag, bool>>? filter = null;

        if (!string.IsNullOrWhiteSpace(request.SearchName))
        {
            var searchTerm = request.SearchName.ToLower();
            filter = t => t.Name.ToLower().Contains(searchTerm);
        }

        var tags = await tagRepository.GetAllAsync(
            expression: filter,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        return tags.ToPagedListDto(t => t.ToResponse());
    }
}
