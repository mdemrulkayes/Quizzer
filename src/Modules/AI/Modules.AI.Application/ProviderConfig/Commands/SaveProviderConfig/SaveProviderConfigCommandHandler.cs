using Modules.AI.Application.Dtos;
using Modules.AI.Core.Models;
using Modules.AI.Core.Repositories;
using Modules.AI.Core.Security;
using Shared.Core;

namespace Modules.AI.Application.ProviderConfig.Commands.SaveProviderConfig;

internal sealed class SaveProviderConfigCommandHandler(
    IAIProviderConfigRepository repository,
    IApiKeyEncryptionService encryptionService,
    IUser user)
    : ICommandHandler<SaveProviderConfigCommand, Result<ProviderConfigResponse>>
{
    private static readonly Dictionary<string, string> ProviderNames = new()
    {
        ["gemini"] = "Google Gemini",
        ["groq"] = "Groq (Llama 3)"
    };

    public async Task<Result<ProviderConfigResponse>> Handle(
        SaveProviderConfigCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            return Error.Unauthorized("User.NotAuthenticated", "User is not authenticated.");
        }

        var userId = Guid.Parse(user.Id);
        var encryptedKey = encryptionService.Encrypt(request.SecretKey);
        var providerName = ProviderNames.GetValueOrDefault(request.ProviderId, request.ProviderId);

        var existing = await repository.GetByUserIdAsync(userId, cancellationToken);

        if (existing is not null)
        {
            existing.ProviderId = request.ProviderId;
            existing.ProviderName = providerName;
            existing.EncryptedSecretKey = encryptedKey;
            existing.IsActive = true;
            existing.ConfiguredAt = DateTimeOffset.UtcNow;
            existing.LastTestedAt = null;
            existing.LastTestResult = null;
        }
        else
        {
            existing = new AIProviderConfig
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProviderId = request.ProviderId,
                ProviderName = providerName,
                EncryptedSecretKey = encryptedKey,
                IsActive = true,
                ConfiguredAt = DateTimeOffset.UtcNow
            };
        }

        await repository.SaveAsync(existing, cancellationToken);

        var response = new ProviderConfigResponse(
            existing.Id,
            existing.ProviderId,
            existing.ProviderName,
            existing.IsActive,
            MaskedApiKey: "****...configured",
            existing.ConfiguredAt,
            existing.LastTestedAt,
            existing.LastTestResult);

        return response;
    }
}
