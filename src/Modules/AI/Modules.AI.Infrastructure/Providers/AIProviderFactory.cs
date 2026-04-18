using Modules.AI.Core.Errors;
using Modules.AI.Core.Providers;
using Modules.AI.Core.Repositories;
using Modules.AI.Core.Security;
using Shared.Core;

namespace Modules.AI.Infrastructure.Providers;

public class AIProviderFactory(
    IAIProviderConfigRepository configRepository,
    IApiKeyEncryptionService encryptionService,
    IUser currentUser,
    GeminiProvider geminiProvider,
    GroqProvider groqProvider) : IAIProviderFactory
{
    public async Task<Result<(IAIProvider Provider, string DecryptedKey)>> ResolveForCurrentUserAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentUser.Id is null)
            return Error.Unauthorized("AIProvider.Unauthorized", "User is not authenticated.");

        var config = await configRepository.GetByUserIdAsync(
            Guid.Parse(currentUser.Id), cancellationToken);

        if (config is null)
            return AIProviderErrors.ProviderNotConfigured;

        var decryptedKey = encryptionService.Decrypt(config.EncryptedSecretKey);

        IAIProvider provider = config.ProviderId switch
        {
            "gemini" => geminiProvider,
            "groq" => groqProvider,
            _ => throw new InvalidOperationException($"Unknown provider: {config.ProviderId}")
        };

        return (provider, decryptedKey);
    }
}
