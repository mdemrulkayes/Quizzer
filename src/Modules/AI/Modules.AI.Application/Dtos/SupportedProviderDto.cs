namespace Modules.AI.Application.Dtos;

public sealed record SupportedProviderDto(
    string ProviderId,
    string ProviderName,
    string Description,
    string DefaultModel);
