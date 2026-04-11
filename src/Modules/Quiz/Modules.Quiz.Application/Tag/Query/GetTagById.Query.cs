using Modules.Quiz.Application.Tag.Dtos;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Tag.Query;
public sealed record GetTagByIdQuery(long TagId) : IQuery<Result<TagResponse>>, ICacheableQuery
{
    public string CacheKey => $"{CacheKeys.Tags}:id:{TagId}";
}