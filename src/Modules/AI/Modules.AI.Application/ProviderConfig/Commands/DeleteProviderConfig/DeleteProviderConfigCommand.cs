using Shared.Core;

namespace Modules.AI.Application.ProviderConfig.Commands.DeleteProviderConfig;

public sealed record DeleteProviderConfigCommand : ICommand<Result<bool>>;
