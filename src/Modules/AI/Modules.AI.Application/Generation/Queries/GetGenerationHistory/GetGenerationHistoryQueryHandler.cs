using Modules.AI.Application.Dtos;
using Modules.AI.Core.Repositories;
using Shared.Core;

namespace Modules.AI.Application.Generation.Queries.GetGenerationHistory;

internal sealed class GetGenerationHistoryQueryHandler(
    IAIGenerationRequestRepository repository,
    IUser user)
    : IQueryHandler<GetGenerationHistoryQuery, Result<List<GenerationHistoryItemDto>>>
{
    public async Task<Result<List<GenerationHistoryItemDto>>> Handle(
        GetGenerationHistoryQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            return Error.Unauthorized("User.NotAuthenticated", "User is not authenticated.");
        }

        var userId = Guid.Parse(user.Id);
        var items = await repository.GetByUserIdAsync(userId, request.PageNumber, request.PageSize, cancellationToken);

        var dtos = items.Select(item => new GenerationHistoryItemDto(
            item.Id,
            item.Source.ToString().ToLowerInvariant(),
            item.OutputType.ToString().ToLowerInvariant(),
            item.Status.ToString().ToLowerInvariant(),
            item.ErrorMessage,
            item.CreatedAt,
            item.CompletedAt)).ToList();

        return dtos;
    }
}
