using Modules.AI.Core.Providers;
using Modules.AI.Core.Repositories;
using Modules.AI.Core.Security;
using Shared.Core;

namespace Quizzer.Api.FunctionalTest.Mocks;

public class MockAIProviderFactory(
    IAIProviderConfigRepository configRepository,
    IApiKeyEncryptionService encryptionService,
    IUser user) : IAIProviderFactory
{
    public async Task<Result<(IAIProvider Provider, string DecryptedKey)>> ResolveForCurrentUserAsync(
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            return Error.Unauthorized("User.NotAuthenticated", "User is not authenticated.");
        }

        var userId = Guid.Parse(user.Id);
        var config = await configRepository.GetByUserIdAsync(userId, cancellationToken);

        if (config is null)
        {
            return Error.NotFound("AIProvider.NotConfigured",
                "No AI provider has been configured. Please set up your AI provider in settings.");
        }

        var decryptedKey = encryptionService.Decrypt(config.EncryptedSecretKey);
        IAIProvider provider = new MockGeminiProvider();

        return (provider, decryptedKey);
    }
}
