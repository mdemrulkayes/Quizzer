using Modules.AI.Application.Dtos;
using Shared.Core;

namespace Modules.AI.Application.ProviderConfig.Queries.GetProviderConfig;

public sealed record GetProviderConfigQuery : IQuery<Result<ProviderConfigResponse>>;
