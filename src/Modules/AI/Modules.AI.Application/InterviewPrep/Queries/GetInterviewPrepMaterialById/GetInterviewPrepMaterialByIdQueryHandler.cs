using Modules.AI.Application.Dtos;
using Modules.AI.Core.Errors;
using Modules.AI.Core.Repositories;
using Shared.Core;

namespace Modules.AI.Application.InterviewPrep.Queries.GetInterviewPrepMaterialById;

internal sealed class GetInterviewPrepMaterialByIdQueryHandler(
    IInterviewPrepMaterialRepository repository,
    IUser user)
    : IQueryHandler<GetInterviewPrepMaterialByIdQuery, Result<InterviewPrepMaterialDetailDto>>
{
    public async Task<Result<InterviewPrepMaterialDetailDto>> Handle(
        GetInterviewPrepMaterialByIdQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            return Error.Unauthorized("User.NotAuthenticated", "User is not authenticated.");
        }

        var material = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (material is null)
        {
            return AIGenerationErrors.InterviewPrepNotFound;
        }

        var userId = Guid.Parse(user.Id);
        if (material.UserId != userId)
        {
            return AIGenerationErrors.InterviewPrepNotFound;
        }

        var dto = new InterviewPrepMaterialDetailDto(
            material.Id,
            material.JobTitle,
            material.JobDescription,
            material.KeyTopics,
            material.ReadingMaterials.Select(r => new ReadingMaterialDto(
                r.Title, r.Description, r.Url, r.Type)).ToList(),
            material.PracticeQuestions.Select(p => new PracticeQuestionDto(
                p.Question, p.Hint)).ToList(),
            material.PreparationTips,
            material.CreatedAt);

        return dto;
    }
}
