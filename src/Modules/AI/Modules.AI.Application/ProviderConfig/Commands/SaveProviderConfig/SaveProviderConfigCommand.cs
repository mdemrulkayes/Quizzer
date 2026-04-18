using Modules.AI.Application.Dtos;
using Shared.Core;

namespace Modules.AI.Application.ProviderConfig.Commands.SaveProviderConfig;

public sealed record SaveProviderConfigCommand(string ProviderId, string SecretKey)
    : ICommand<Result<ProviderConfigResponse>>;
