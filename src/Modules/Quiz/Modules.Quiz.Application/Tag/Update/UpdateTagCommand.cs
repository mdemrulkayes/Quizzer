using Modules.Quiz.Application.Tag.Dtos;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Tag.Update;

public sealed record UpdateTagCommand(long TagId, string Name, string Description) : ICommand<Result<TagResponse>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate => [$"{CacheKeys.Tags}:id:{TagId}"];
}
