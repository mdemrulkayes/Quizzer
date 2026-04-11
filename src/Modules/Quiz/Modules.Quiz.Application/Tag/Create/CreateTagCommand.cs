using Modules.Quiz.Application.Tag.Dtos;
using Shared.Core;
using Shared.Core.Caching;

namespace Modules.Quiz.Application.Tag.Create;

public sealed record CreateTagCommand(string Name, string? Description) : ICommand<Result<TagResponse>>, ICacheInvalidatingCommand
{
    public string[] CacheKeysToInvalidate => [$"{CacheKeys.Tags}:all:"];
}
