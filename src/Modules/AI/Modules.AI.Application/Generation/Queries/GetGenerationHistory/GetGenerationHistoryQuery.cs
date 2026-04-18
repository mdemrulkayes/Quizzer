using Modules.AI.Application.Dtos;
using Shared.Core;

namespace Modules.AI.Application.Generation.Queries.GetGenerationHistory;

public sealed record GetGenerationHistoryQuery(int PageNumber = 1, int PageSize = 10)
    : IQuery<Result<List<GenerationHistoryItemDto>>>;
