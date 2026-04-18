using Modules.AI.Application.Dtos;
using Modules.AI.Core.Repositories;
using Shared.Core;

namespace Modules.AI.Application.InterviewPrep.Queries.GetInterviewPrepMaterials;

internal sealed class GetInterviewPrepMaterialsQueryHandler(
    IInterviewPrepMaterialRepository repository,
    IUser user)
    : IQueryHandler<GetInterviewPrepMaterialsQuery, Result<List<InterviewPrepMaterialDto>>>
{
    public async Task<Result<List<InterviewPrepMaterialDto>>> Handle(
        GetInterviewPrepMaterialsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            return Error.Unauthorized("User.NotAuthenticated", "User is not authenticated.");
        }

        var userId = Guid.Parse(user.Id);
        var items = await repository.GetByUserIdAsync(userId, request.PageNumber, request.PageSize, cancellationToken);

        var dtos = items.Select(item => new InterviewPrepMaterialDto(
            item.Id,
            item.JobTitle,
            item.KeyTopics,
            item.CreatedAt)).ToList();

        return dtos;
    }
}
