using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Tag.Delete;
public sealed record DeleteTagCommand(long TagId) : ICommand<Result<bool>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate => [$"{CacheKeys.Tags}:id:{TagId}"];
}
