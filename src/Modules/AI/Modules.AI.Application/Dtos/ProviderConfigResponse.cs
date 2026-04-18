namespace Modules.AI.Application.Dtos;

public sealed record ProviderConfigResponse(
    Guid Id,
    string ProviderId,
    string ProviderName,
    bool IsActive,
    string MaskedApiKey,
    DateTimeOffset ConfiguredAt,
    DateTimeOffset? LastTestedAt,
    string? LastTestResult);
