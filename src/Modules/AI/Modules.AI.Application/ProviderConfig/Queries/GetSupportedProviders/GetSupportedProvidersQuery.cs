using Modules.AI.Application.Dtos;
using Shared.Core;

namespace Modules.AI.Application.ProviderConfig.Queries.GetSupportedProviders;

public sealed record GetSupportedProvidersQuery : IQuery<Result<List<SupportedProviderDto>>>;
