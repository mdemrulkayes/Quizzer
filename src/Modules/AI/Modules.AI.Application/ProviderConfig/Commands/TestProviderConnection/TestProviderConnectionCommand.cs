using Modules.AI.Application.Dtos;
using Shared.Core;

namespace Modules.AI.Application.ProviderConfig.Commands.TestProviderConnection;

public sealed record TestProviderConnectionCommand : ICommand<Result<TestConnectionResponse>>;
