using Modules.AI.Application.Dtos;
using Modules.AI.Core.Errors;
using Modules.AI.Core.Repositories;
using Shared.Core;

namespace Modules.AI.Application.ProviderConfig.Queries.GetProviderConfig;

internal sealed class GetProviderConfigQueryHandler(
    IAIProviderConfigRepository repository,
    IUser user)
    : IQueryHandler<GetProviderConfigQuery, Result<ProviderConfigResponse>>
{
    public async Task<Result<ProviderConfigResponse>> Handle(
        GetProviderConfigQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            return Error.Unauthorized("User.NotAuthenticated", "User is not authenticated.");
        }

        var userId = Guid.Parse(user.Id);
        var config = await repository.GetByUserIdAsync(userId, cancellationToken);

        if (config is null)
        {
            return AIProviderErrors.ProviderNotConfigured;
        }

        var response = new ProviderConfigResponse(
            config.Id,
            config.ProviderId,
            config.ProviderName,
            config.IsActive,
            MaskedApiKey: "****...configured",
            config.ConfiguredAt,
            config.LastTestedAt,
            config.LastTestResult);

        return response;
    }
}
