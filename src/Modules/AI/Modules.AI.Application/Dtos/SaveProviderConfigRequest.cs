namespace Modules.AI.Application.Dtos;

public sealed record SaveProviderConfigRequest(
    string ProviderId,
    string SecretKey);
