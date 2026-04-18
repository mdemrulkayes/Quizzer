using Shared.Core;

namespace Modules.AI.Core.Providers;

public interface IAIProvider
{
    string ProviderId { get; }
    Task<Result<string>> GenerateAsync(
        string systemPrompt,
        string userPrompt,
        string decryptedApiKey,
        CancellationToken cancellationToken = default);
    Task<Result<bool>> TestConnectionAsync(
        string decryptedApiKey,
        CancellationToken cancellationToken = default);
}
