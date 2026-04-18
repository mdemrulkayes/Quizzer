using Shared.Core;

namespace Modules.AI.Core.Providers;

public interface IAIProviderFactory
{
    Task<Result<(IAIProvider Provider, string DecryptedKey)>> ResolveForCurrentUserAsync(
        CancellationToken cancellationToken = default);
}
